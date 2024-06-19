namespace IMFINE.Net
{
    public class NetworkInfo
    {
        public string networkName;
        public string IpAddress;
        public string networkId;

        public NetworkInfo(string name, string ip, string id)
        {
            networkName = name.Replace("이더넷", "Ethernet");
            IpAddress = ip;
            networkId = id;
        }
    }
}