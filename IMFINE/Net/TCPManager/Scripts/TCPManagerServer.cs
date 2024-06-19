namespace IMFINE.Net.TCPManager
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using IMFINE.Utils;

    public class TCPManagerServer : MonoSingleton<TCPManagerServer>
    {
        public event Action<ETCPStatus> StatusChanged;
        public event Action<string> ClientRemoved;
        public event Action ClientStatusUpdated;

        public delegate void TCPMessageEvent(CMDType cmdType, string receiverId, string message, string argument, string senderId);
        public event TCPMessageEvent MessageReceived;
        public event TCPMessageEvent MessageSent;

        public BaseClient serverBaseClient = new BaseClient(null);
        List<BaseClient> baseClientList = new();

        TcpListener tcpListener;
        SynchronizationContext context;
        bool isRunning = false;

        public List<BaseClient> BaseClientList => baseClientList;

        bool IsPlaying => Application.IsPlaying(gameObject);

        bool EnableDetailedLog => TCPManager.instance.EnableDetailedLog;

        bool isAcceptTaskRunning, isPingCoroutineIsRunning;

        public bool Open(string id, string ip, int port)
        {
            if (isRunning) return false;
            try
            {
                serverBaseClient.id = id;
                serverBaseClient.ip = ip;
                serverBaseClient.port = port;
                serverBaseClient.isOnline = true;

                context = SynchronizationContext.Current;
                tcpListener = new TcpListener(IPAddress.Parse(ip), port);
                tcpListener.Start();

                isRunning = true;
                StatusChanged?.Invoke(ETCPStatus.Running);
                Debug.Log("> " + GetType().Name + " / Open Server Success / " + ip + ":" + port);

                if (!isAcceptTaskRunning) _ = AcceptClientsAsync();
                if (!isPingCoroutineIsRunning) StartCoroutine("StartPing");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("> " + GetType().Name + " / Open Server Fail / [Error]: " + ex.Message);
                return false;
            }
        }

        public void ShutdownServer()
        {
            try
            {
                tcpListener?.Stop();
                DisconnectAllClients(false);

                isRunning = false;
                StatusChanged?.Invoke(ETCPStatus.NotRunning);
                ClientStatusUpdated?.Invoke();
                Debug.Log("> " + GetType().Name + " / Shutdown Server Success");
            }
            catch (Exception ex)
            {
                Debug.Log("> " + GetType().Name + " / Shutdown Server Fail / [Error]: " + ex.Message);
            }
        }

        public void DisconnectClient(BaseClient baseClient, bool isForce, bool isRemovedFromList = true, bool isSendDisconnect = true)
        {
            baseClient.isOnline = false;
            baseClient.cancelTokenSource.Cancel();
            if (isSendDisconnect) SendWithAddress(CMDType.ServerDisconnect, baseClient.ip, baseClient.port, "", isForce ? "true" : "false");

            if (isRemovedFromList) baseClientList.Remove(baseClient);
            ClientRemoved?.Invoke(baseClient.id);
        }

        public void DisconnectClientWithId(string clientId, bool isForce, bool isRemovedFromList = true)
        {
            BaseClient clientToDisconnect = baseClientList.Find(client => client.id == clientId);
            if (clientToDisconnect != null)
            {
                DisconnectClient(clientToDisconnect, isForce, isRemovedFromList);
            }
        }

        public void DisconnectAllClients(bool isForce, bool isRemovedFromList = true)
        {
            foreach (var baseClient in baseClientList)
            {
                try
                {
                    DisconnectClient(baseClient, isForce, false);
                }
                catch { }
            }
            if (isRemovedFromList)
            {
                baseClientList.Clear();
                ClientStatusUpdated?.Invoke();
            }
        }

        /// <param name="receiverId">id or ALL or OTHERS</param>
        public void Send(CMDType cmdType, string receiverId, string messageString, string argument, string senderId = "")
        {
            if (!isRunning)
            {
                if (EnableDetailedLog) Debug.Log("> " + GetType().Name + " / Send Fail / Not Running");
                return;
            }

            if (senderId == "") senderId = serverBaseClient.id;
            bool isServerReceiver = false;
            List<BaseClient> receiverBaseClients;

            if (receiverId == "[ALL]")
            {
                receiverBaseClients = baseClientList;
                isServerReceiver = true;
            }
            else if (receiverId == "[OTHERS]")
            {
                receiverBaseClients = baseClientList.FindAll(x => x.id != senderId);
                if (senderId != serverBaseClient.id) isServerReceiver = true;
            }
            else
            {
                receiverBaseClients = baseClientList.FindAll(x => x.id == receiverId);
                if (receiverBaseClients.Count == 0)
                {
                    if (EnableDetailedLog) Debug.Log("> " + GetType().Name + $" / Send Fail / [{receiverId}] is not found");
                }
            }
            foreach (BaseClient receiverBaseClient in receiverBaseClients)
            {
                SendAsync(cmdType, receiverBaseClient, messageString, argument, senderId);
            }
            if (isServerReceiver) SendAsync(cmdType, serverBaseClient, messageString, argument, senderId);
        }

        public void SendWithAddress(CMDType cmdType, string receiverIp, int receiverPort, string messageString, string argument = "")
        {
            if (!isRunning)
            {
                if (EnableDetailedLog) Debug.Log("> " + GetType().Name + " / Send With Address Fail / Not Running");
                return;
            }
            BaseClient receiverBaseClient = baseClientList.Find(x => x.ip == receiverIp && x.port == receiverPort);
            if (receiverBaseClient != null) SendAsync(cmdType, receiverBaseClient, messageString, argument, serverBaseClient.id);
            else if (EnableDetailedLog) Debug.Log("> " + GetType().Name + $" / Send With Address Fail / [{receiverIp}:{receiverPort}] is not found");
        }

        async Task AcceptClientsAsync()
        {
            isAcceptTaskRunning = true;
            while (IsPlaying && isRunning)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();

                    IPEndPoint ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    if (serverBaseClient.ip == ipEndPoint.Address.ToString() && serverBaseClient.port == ipEndPoint.Port)
                    {
                        continue;
                    }
                    BaseClient baseClient = new BaseClient(client)
                    {
                        ip = ipEndPoint.Address.ToString(),
                        port = ipEndPoint.Port,
                        isOnline = true
                    };

                    lock (baseClientList) baseClientList.Add(baseClient);
                    Debug.Log("> " + GetType().Name + " / AcceptClientsAsync / Client Connected / " + ipEndPoint.Address + ":" + ipEndPoint.Port);

                    baseClient.cancelTokenSource = new CancellationTokenSource();
                    _ = ReadClientAsync(baseClient, baseClient.cancelTokenSource.Token);

                    context.Post(_ => SendWithAddress(CMDType.ServerConnect, baseClient.ip, baseClient.port, serverBaseClient.Serialize()), null);
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
            isAcceptTaskRunning = false;
        }

        async Task ReadClientAsync(BaseClient baseClient, CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream stream = baseClient.client.GetStream();
                while (IsPlaying && isRunning)
                {
                    if (!baseClient.client.Connected) break;
                    byte[] lengthBuffer = new byte[4];

                    int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, cancellationToken);
                    if (bytesRead == 0)
                    {
                        if (baseClient.isOnline)
                        {
                            baseClient.isOnline = false;
                            context.Post(_ => ClientStatusUpdated?.Invoke(), null);
                        }
                        break;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] buffer = new byte[messageLength];

                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        if (baseClient.isOnline)
                        {
                            baseClient.isOnline = false;
                            context.Post(_ => ClientStatusUpdated?.Invoke(), null);
                        }
                        break;
                    }

                    if (!baseClient.isOnline)
                    {
                        baseClient.isOnline = true;
                        context.Post(_ => ClientStatusUpdated?.Invoke(), null);
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] messages = message.Split('|'); if (messages.Length >= 5)
                    {
                        context.Post(_ => MessageReceived?.Invoke(Enum.Parse<CMDType>(messages[0]), messages[1], messages[2], messages[3], messages[4]), null);
                    }
                    else
                    {
                        Debug.Log("> " + GetType().Name + " / ReadClientAsync / Message received parsing fail raw: " + message);
                    }
                }
            }
            catch { }
            finally
            {
                context.Post(_ => DisconnectClientWithId(baseClient.id, false, false), null);
            }
        }

        async void SendAsync(CMDType cmdType, BaseClient receiverBaseClient, string messageString, string argument, string senderId)
        {
            try
            {
                if (receiverBaseClient != serverBaseClient && !receiverBaseClient.client.Connected)
                {
                    if (EnableDetailedLog) Debug.Log("> " + GetType().Name + " / CMDType [" + cmdType + "] / Send Fail / [" + receiverBaseClient.id + "] is not connected");
                    return;
                }
                string packet = $"{cmdType}|{receiverBaseClient.id}|{messageString}|{argument}|{senderId}";
                byte[] messageBuffer = Encoding.UTF8.GetBytes(packet);
                byte[] lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);
                byte[] buffer = new byte[lengthBuffer.Length + messageBuffer.Length];
                Buffer.BlockCopy(lengthBuffer, 0, buffer, 0, lengthBuffer.Length);
                Buffer.BlockCopy(messageBuffer, 0, buffer, lengthBuffer.Length, messageBuffer.Length);

                if (receiverBaseClient != serverBaseClient)
                {
                    NetworkStream stream = receiverBaseClient.client.GetStream();
                    await stream.WriteAsync(buffer, 0, buffer.Length);

                    if (cmdType == CMDType.ServerDisconnect)
                    {
                        receiverBaseClient.client.Close();
                        Debug.Log("> " + GetType().Name + " / Client [" + receiverBaseClient.id + "] is disconnected.");
                    }
                    context.Post(_ => MessageSent?.Invoke(cmdType, receiverBaseClient.id, messageString, argument, senderId), null);

                }
                else
                {
                    context.Post(_ => MessageReceived?.Invoke(cmdType, receiverBaseClient.id, messageString, argument, senderId), null);
                }
            }
            catch { }
        }

        IEnumerator StartPing()
        {
            isPingCoroutineIsRunning = true;
            float checkIntervalSec = 0.1f;
            float pingTimeoutSec = 2f;
            WaitForSeconds waitForCheckSec = new WaitForSeconds(checkIntervalSec);
            WaitForSeconds waitForPingSec = new WaitForSeconds(pingTimeoutSec);

            float currentPingWaitTime;
            bool isConnectionError;

            while (IsPlaying && isRunning)
            {
                isConnectionError = false;
                currentPingWaitTime = 0;
                Ping p = new Ping(serverBaseClient.ip);

                while (!p.isDone)
                {
                    yield return waitForCheckSec;
                    currentPingWaitTime += checkIntervalSec;
                    if (p.time < 0 || currentPingWaitTime >= pingTimeoutSec)
                    {
                        isConnectionError = true;
                        break;
                    }
                }

                if (isConnectionError)
                {
                    p.DestroyPing();
                    if (TCPManager.instance.ETCPStatus == ETCPStatus.Running)
                    {
                        StatusChanged?.Invoke(ETCPStatus.ConnectionError);
                        tcpListener?.Stop();
                        Debug.Log("> " + GetType().Name + " / Connection status is not good. Trying to connect again...");
                    }
                }
                else
                {
                    if (TCPManager.instance.ETCPStatus == ETCPStatus.ConnectionError)
                    {
                        StatusChanged?.Invoke(ETCPStatus.Running);
                        tcpListener?.Start();
                        Debug.Log("> " + GetType().Name + " / Connection status has returned to running!");
                    }
                }
                yield return waitForPingSec;
            }
            isPingCoroutineIsRunning = false;
        }

        void OnApplicationQuit()
        {
            ShutdownServer();
        }

    }
}