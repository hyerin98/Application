namespace IMFINE.Net
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.NetworkInformation;
	using System.Net.Sockets;
	using UnityEngine;
	using UnityEngine.Android;

	public class NetworkUtility
	{

		public static string GetLocalIPAddress()
		{
			IPHostEntry host;

			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			return null;
		}


		public static string GetGatewayIPAddress()
		{
			try
			{
				NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

				foreach (NetworkInterface networkInterface in networkInterfaces)
				{
					if (networkInterface.OperationalStatus == OperationalStatus.Up)
					{
						GatewayIPAddressInformation gatewayInfo = networkInterface.GetIPProperties()
							.GatewayAddresses.FirstOrDefault();

						if (gatewayInfo != null)
						{
							return gatewayInfo.Address.ToString();
						}
					}
				}
			}
			catch { }

			string[] octets = GetLocalIPAddress().Split('.');
			octets[3] = "1";
			return string.Join(".", octets);
		}

		public static List<NetworkInfo> GetLocalNetworkInfoList()
		{
			List<NetworkInfo> networkInfoList = new List<NetworkInfo>();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			// 모든 네트워크 인터페이스 가져오기
			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			// 각 인터페이스에서 IPv4 주소 가져오기
			foreach (NetworkInterface networkInterface in networkInterfaces)
			{
				// 인터페이스가 활성화되었고 루프백이 아닌 경우
				if (networkInterface.OperationalStatus == OperationalStatus.Up &&
					networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
				{
					// IPv4 주소 가져오기
					foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							// 로컬 IPv4 주소 출력
							networkInfoList.Add(new NetworkInfo(networkInterface.Name, ip.Address.ToString(), networkInterface.Id));
						}
					}
				}
			}
#elif UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX
			IPHostEntry host;
			host = Dns.GetHostEntry(Dns.GetHostName());
			int count = 1;
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					networkInfoList.Add(new NetworkInfo($"Network"+count, ip.ToString(), "{ID is not provided by this platform}"));
					count++;
				}
			}
#endif
			return networkInfoList;
		}

		public static bool IsValidIPv4Address(string address)
		{
			string pattern = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
			// 입력된 주소가 IP 주소 형식인지 확인
			if (System.Text.RegularExpressions.Regex.IsMatch(address, pattern))
			{
				IPAddress ip;
				// 주어진 문자열을 IP 주소로 변환
				if (IPAddress.TryParse(address, out ip))
				{
					// 주소가 IPv4 주소인지 확인
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						byte[] bytes = ip.GetAddressBytes();
						// 각 구성 요소가 0부터 255 사이의 값인지 확인
						if (bytes[0] >= 224 && bytes[0] <= 239)
						{
							return false;
						}
						foreach (byte b in bytes)
						{
							if (b < 0 || b > 255)
							{
								return false;
							}
						}
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsMulticastAddress(string address)
		{
			string pattern = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
			// 입력된 주소가 IP 주소 형식인지 확인
			if (System.Text.RegularExpressions.Regex.IsMatch(address, pattern))
			{
				IPAddress ip;
				if (IPAddress.TryParse(address, out ip))
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						byte[] bytes = ip.GetAddressBytes();
						if (bytes[0] >= 224 && bytes[0] <= 239)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}
	}
}