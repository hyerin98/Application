namespace IMFINE.Net.TCPManager
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using IMFINE.Utils;
    using UnityEngine;

    public class TCPManagerClient : MonoSingleton<TCPManagerClient>
    {
        public event Action<ETCPStatus> StatusChanged;
        public event Action<string> ServerDisconnected;
        public event Action ServerStatusUpdated;

        public delegate void TCPMessageEvent(CMDType cmdType, string receiverId, string message, string argument, string senderId);
        public event TCPMessageEvent MessageReceived;
        public event TCPMessageEvent MessageSent;

        public BaseClient baseClient;
        public BaseClient serverBaseClient;

        SynchronizationContext context;
        bool isTryingConnect;
        bool isConnected;

        public IPEndPoint IPEndPoint => baseClient?.client?.Client?.LocalEndPoint as IPEndPoint;

        bool IsPlaying => Application.IsPlaying(gameObject);

        bool EnableDetailedLog => TCPManager.instance.EnableDetailedLog;

        public void StartConnect(string id, string serverIp, int serverPort)
        {
            if (isTryingConnect || isConnected) return;
            isTryingConnect = true;
            StatusChanged?.Invoke(ETCPStatus.TryingConnect);
            context = SynchronizationContext.Current;
            _ = TryConnectAsync(id, serverIp, serverPort);
        }

        public void Disconnect()
        {
            CloseClient();
            isTryingConnect = false;
            isConnected = false;
            context?.Post(_ =>
            {
                if (serverBaseClient != null)
                {
                    ServerDisconnected?.Invoke(serverBaseClient.id);
                    serverBaseClient.Clear();
                }
                StatusChanged?.Invoke(ETCPStatus.Disconnected);
            }, null);
        }

        public void Send(CMDType cmdType, string receverId, string messageString, string argument = "")
        {
            if (!isConnected)
            {
                if (EnableDetailedLog) Debug.Log("> " + GetType().Name + $" / {cmdType} Send Fail / Not Connected");
                return;
            }
            string senderId = baseClient.id;
            if (receverId == "") receverId = serverBaseClient.id;
            SendAsync(cmdType, receverId, senderId, messageString, argument);
        }

        async void SendAsync(CMDType cmdType, string receverId, string senderId, string messageString, string argument = "")
        {
            try
            {
                string packet = $"{cmdType}|{receverId}|{messageString}|{argument}|{senderId}";
                byte[] messageBuffer = Encoding.UTF8.GetBytes(packet);
                byte[] lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
                byte[] buffer = new byte[lengthBuffer.Length + messageBuffer.Length];
                Buffer.BlockCopy(lengthBuffer, 0, buffer, 0, lengthBuffer.Length);
                Buffer.BlockCopy(messageBuffer, 0, buffer, lengthBuffer.Length, messageBuffer.Length);

                if (baseClient.client != null)
                {
                    NetworkStream stream = baseClient.client.GetStream();
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    context.Post(_ => MessageSent?.Invoke(cmdType, receverId, messageString, argument, senderId), null);
                }
            }
            catch (Exception ex)
            {
                if (EnableDetailedLog) Debug.Log("> " + GetType().Name + " / Send Fail / " + ex.Message);
            }
        }

        async Task TryConnectAsync(string id, string serverIp, int serverPort)
        {
            while (IsPlaying && isTryingConnect)
            {
                if (!isConnected)
                {
                    try
                    {
                        if (EnableDetailedLog) Debug.Log("> " + GetType().Name + " / Try connect..");
                        CloseClient(false);
                        baseClient = new BaseClient(new TcpClient());
                        Task connectTask = baseClient.client.ConnectAsync(serverIp, serverPort);
                        Task completeTask = await Task.WhenAny(connectTask, Task.Delay(1000));

                        if (connectTask == completeTask && connectTask.IsCompletedSuccessfully)
                        {
                            serverBaseClient = new BaseClient(null)
                            {
                                ip = serverIp,
                                port = serverPort,
                                isOnline = true
                            };

                            baseClient.id = id;
                            baseClient.ip = IPEndPoint.Address.ToString();
                            baseClient.port = IPEndPoint.Port;

                            isConnected = true;
                            context.Post(_ => StatusChanged?.Invoke(ETCPStatus.Connected), null);
                            Debug.Log("> " + GetType().Name + " / Connected to Server / " + serverIp + ":" + serverPort);

                            _ = ReadMessagesAsync();
                            StartCoroutine("StartPing");
                        }
                        else
                        {
                            CloseClient(false);
                            await Task.Delay(1000);
                        }
                    }
                    catch
                    {
                        CloseClient(false);
                        await Task.Delay(1000);
                        continue;
                    }
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        }

        async Task ReadMessagesAsync()
        {
            try
            {
                while (IsPlaying && isConnected)
                {
                    if (!baseClient.client.Connected) break;

                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = await baseClient.client.GetStream().ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] buffer = new byte[messageLength];

                    bytesRead = await baseClient.client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] messages = message.Split('|');
                    if (messages.Length >= 5)
                    {
                        context.Post(_ => MessageReceived?.Invoke(Enum.Parse<CMDType>(messages[0]), messages[1], messages[2], messages[3], messages[4]), null);
                    }
                    else Debug.Log("> " + GetType().Name + " / ReadClientAsync / Message received parsing fail raw: " + message);
                }
            }
            catch { }
            finally
            {
                serverBaseClient.isOnline = false;
            }
        }

        IEnumerator StartPing()
        {
            float pingTimeoutSec = 2f;
            WaitForSeconds waitTimeoutSec = new WaitForSeconds(pingTimeoutSec);
            while (IsPlaying && isConnected)
            {
                Ping p = new Ping(serverBaseClient.ip);
                yield return waitTimeoutSec;

                if (!IsPlaying || !isConnected) yield break;

                if (!serverBaseClient.isOnline || p.time < 0 || p.isDone == false)
                {
                    p.DestroyPing();
                    serverBaseClient.isOnline = false;
                    isConnected = false;
                    ServerStatusUpdated?.Invoke();
                    StatusChanged?.Invoke(ETCPStatus.ConnectionError);
                    Debug.Log("> " + GetType().Name + " / Connection status is not good. Trying to connect again...");
                    break;
                }
            }
        }

        void CloseClient(bool closeStream = true)
        {
            try
            {
                if (closeStream) baseClient?.client?.GetStream()?.Close();
            }
            catch { }
            finally
            {
                baseClient?.client?.Close();
                baseClient = null;
            }
        }

        void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}