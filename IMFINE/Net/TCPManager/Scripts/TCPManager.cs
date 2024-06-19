namespace IMFINE.Net.TCPManager
{
#pragma warning disable
    using IMFINE.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public enum ETCPStatus
    {
        NotRunning,
        Running,
        Disconnected,
        TryingConnect,
        Connected,
        ConnectionError,
    }

    public enum CMDType
    {
        Message,
        ServerConnect,
        ServerConnectRecv,
        ServerDisconnect,
        ServerBroadcast,
        TimeSyncRequest,
        TimeSyncRecv,
        TimeSyncMessage,
        TimeSyncUI,
    }

    public class TimeSyncMessage
    {
        public string message;
        public string argument;
        public long unixTime;
        public string senderId;

        public TimeSyncMessage(string _message, string _argument, long _unixTime, string _senderId)
        {
            message = _message;
            argument = _argument;
            unixTime = _unixTime;
            senderId = _senderId;
        }
    }

    public class TCPManager : MonoSingleton<TCPManager>
    {
        public delegate void TCPManagerEvent();
        public event TCPManagerEvent Initialized;
        public event TCPManagerEvent ConfigUpdated;
        public event TCPManagerEvent GlobalTimeUpdated;
        public event TCPManagerEvent TimeSyncRequested;
        public event TCPManagerEvent ClientStatusUpdated;

        public delegate void TCPBaseClientEvent(string clientId);
        public event TCPBaseClientEvent ClientAdded;
        public event TCPBaseClientEvent ClientRemoved;

        public delegate void TCPStatusEvent(ETCPStatus status);
        public event TCPStatusEvent StatusUpdated;

        public delegate void TCPMessageEvent(string receiverId, string message, string argument, string senderId);
        public event TCPMessageEvent MessageReceived;
        public event TCPMessageEvent MessageSent;

        [SerializeField] private bool _enableAutoConnection = false;
        [SerializeField] private bool _isServer = true;
        [SerializeField] private string _id = "A";
        [SerializeField] private string _serverIp = "192.168.1.1";
        [SerializeField] private int _serverPort = 49157;

        [SerializeField] bool _enableDetailedLog = false;
        [SerializeField] bool _enableMessageLog = true;

        ETCPStatus _eTCPStatus;
        bool _isInitialized;
        long _globalUnixTime;
        int _timeOffset = 0;
        int _syncDelayMs = 300;
        int _syncInterval = 30;
        long _beforeUnixTime;
        long _unixTimeOffset;
        string _configDataPath;

        public bool IsInitialized { get => _isInitialized; }
        public bool IsServer { get => _isServer; }
        public string Id { get => _id; }
        public string ServerIp { get => _serverIp; }
        public int ServerPort { get => _serverPort; }
        public bool EnableAutoConnection { get => _enableAutoConnection; }
        public bool EnableDetailedLog { get => _enableDetailedLog; }
        public ETCPStatus ETCPStatus { get => _eTCPStatus; }
        public long GlobalUnixTime { get => _globalUnixTime; }
        public int TimeOffset { get => _timeOffset; }
        public int SyncDelayMs { get => _syncDelayMs; }
        public int SyncInterval { get => _syncInterval; }
        public bool IsStartConnect { get => !_eTCPStatus.Equals(ETCPStatus.NotRunning) && !_eTCPStatus.Equals(ETCPStatus.Disconnected); }

        private List<TimeSyncMessage> timeSyncMessages = new List<TimeSyncMessage>();

        private void Init()
        {
            if (_isInitialized) return;
            _configDataPath = Path.Combine(ApplicationDirectory.instance.dataDirectoryInfo.FullName, "TCPManagerConfig.csv");
            LoadConfigData();
            OnStatusChanged(IsServer ? ETCPStatus.NotRunning : ETCPStatus.Disconnected);
            _isInitialized = true;
            PrintLog("> " + GetType().Name + " / Initialized");
            Initialized?.Invoke();

            if (_enableAutoConnection)
            {
                StartConnect(_isServer, _serverIp, _serverPort);
            }
        }

        public void ChangeIsServer(bool isServer)
        {
            if (IsStartConnect) return;
            if (isServer != _isServer)
            {
                _isServer = isServer;
                if (isServer) _serverIp = NetworkUtility.GetLocalIPAddress();
                OnStatusChanged(isServer ? ETCPStatus.NotRunning : ETCPStatus.Disconnected);
                SaveConfigData();
            }
        }

        public void ChangeEnableAutoConnection(bool enableAutoConnection)
        {
            if (IsStartConnect) return;
            if (enableAutoConnection != _enableAutoConnection)
            {
                _enableAutoConnection = enableAutoConnection;
                SaveConfigData();
            }
        }

        public bool ChangeId(string id)
        {
            if (IsStartConnect || id == "") return false;
            if (_id != id)
            {
                _id = id;
                SaveConfigData();
            }
            return true;
        }

        public bool ChangeServerIp(string ip)
        {
            if (IsStartConnect || ip == "" || !NetworkUtility.IsValidIPv4Address(ip)) return false;
            if (ip != _serverIp)
            {
                _serverIp = ip;
                SaveConfigData();
            }
            return true;
        }

        public bool ChangeServerPort(int port)
        {
            if (IsStartConnect || port < 49152 || port > 65535) return false;
            if (port != _serverPort)
            {
                _serverPort = port;
                SaveConfigData();
            }
            return true;
        }

        public bool ChangePeriodMinutes(int minutes)
        {
            if (minutes < 1) return false;
            if (minutes != _syncInterval)
            {
                _syncInterval = minutes;
                SaveConfigData();
            }
            return true;
        }

        public bool ChangeTimeOffsetMs(int offset)
        {
            if (offset < 0) return false;
            if (offset != _timeOffset)
            {
                _timeOffset = offset;
                SaveConfigData();
            }
            return true;
        }

        public bool ChangeSyncDelayMs(int delay)
        {
            if (delay < 0) return false;
            if (delay != _syncDelayMs)
            {
                _syncDelayMs = delay;
                SaveConfigData();
            }
            return true;
        }

        public void StartConnect(bool isServer, string serverIp = "", int serverPort = -1)
        {
            if (IsStartConnect)
            {
                PrintLog($"> {GetType().Name} / Fail to start connect / eTCPStatus: {_eTCPStatus}");
                return;
            }
            if (_isServer != isServer || serverIp != "" || serverPort != -1)
            {
                _isServer = isServer;
                _serverIp = serverIp;
                _serverPort = serverPort;
                SaveConfigData();
            }
            _unixTimeOffset = 0;
            if (_isServer) TCPManagerServer.instance.Open(_id, _serverIp, _serverPort);
            else TCPManagerClient.instance.StartConnect(_id, _serverIp, _serverPort);
        }

        public void Disconnect()
        {
            if (!IsStartConnect)
            {
                PrintLog($"> {GetType().Name} / Fail to disconnect / eTCPStatus: {_eTCPStatus}");
                return;
            }

            timeSyncMessages.Clear();
            lastTime = 0;
            if (_isServer) TCPManagerServer.instance.ShutdownServer();
            else TCPManagerClient.instance.Disconnect();
        }

        /// <param name="receiverId">id or ALL or OTHERS</param>
        public void Send(string receiverId, string messageString, string argument = "")
        {
            if (receiverId.ToUpper() == "ALL") receiverId = "[ALL]";
            else if (receiverId.ToUpper() == "OTHERS") receiverId = "[OTHERS]";

            if (_isServer)
            {
                TCPManagerServer.instance.Send(CMDType.Message, receiverId, messageString, argument);
            }
            else
            {
                if (receiverId != "" && receiverId != TCPManagerClient.instance.serverBaseClient.id)
                {
                    // Broadcast
                    TCPManagerClient.instance.Send(CMDType.ServerBroadcast, receiverId, messageString, argument);
                }
                else
                {
                    TCPManagerClient.instance.Send(CMDType.Message, receiverId, messageString, argument);
                }
            }
        }

        public void SendTimeSync(string messageString, string argument = "", int delayMs = -1)
        {
            if (delayMs == -1) delayMs = _syncDelayMs;
            long eventUnixTime = GetGlobalUnixTime() + delayMs;

            if (_isServer)
            {
                TCPManagerServer.instance.Send(CMDType.TimeSyncMessage, "[ALL]", $"{messageString},{eventUnixTime}", argument);
            }
            else
            {
                TCPManagerClient.instance.Send(CMDType.TimeSyncMessage, "[ALL]", $"{messageString},{eventUnixTime}", argument);
            }
        }

        public void RequestTimeSync(string receiverId = "")
        {
            if (_isServer)
            {
                _beforeUnixTime = GetNowUnixTime();
                if (receiverId == "") receiverId = "[OTHERS]";
                TCPManagerServer.instance.Send(CMDType.TimeSyncRequest, receiverId, "", "");
                TimeSyncRequested?.Invoke();
            }
        }

        public void RequestTimeSyncUI(bool isShow)
        {
            if (_isServer) TCPManagerServer.instance.Send(CMDType.TimeSyncUI, "[OTHERS]", "", isShow ? "true" : "false");
            else TCPManagerClient.instance.Send(CMDType.TimeSyncUI, "[OTHERS]", "", isShow ? "true" : "false");
        }




        private void OnStatusChanged(ETCPStatus status)
        {
            if (IsInitialized && _eTCPStatus == status) return;
            _eTCPStatus = status;
            StatusUpdated?.Invoke(status);
        }

        private void OnServerMessageReceived(CMDType cmdType, string receiverId, string message, string argument, string senderId)
        {
            string[] messages = message.Split(',');
            switch (cmdType)
            {
                case CMDType.ServerConnectRecv:
                    string clientId = messages[0];
                    string clientIp = messages[1];
                    int clientPort = int.Parse(messages[2]);

                    BaseClient clientToDisconnect = TCPManagerServer.instance.BaseClientList.Find(x => x.id == clientId);
                    if (clientToDisconnect != null) TCPManagerServer.instance.DisconnectClient(clientToDisconnect, true, true, false);

                    BaseClient currentClient = TCPManagerServer.instance.BaseClientList.Find(x => x.ip == clientIp && x.port == clientPort);
                    if (currentClient != null)
                    {
                        currentClient.id = clientId;
                        ClientAdded?.Invoke(clientId);
                        PrintLog($"> {GetType().Name} / Client Added / clientId: {clientId}");
                    }
                    if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / clientId: {clientId} / senderId: {senderId}");
                    RequestTimeSync(clientId);
                    break;
                case CMDType.ServerBroadcast:
                    TCPManagerServer.instance.Send(CMDType.Message, receiverId, message, argument, senderId);
                    break;
                case CMDType.TimeSyncRequest:
                    BaseClient baseClient = TCPManagerServer.instance.BaseClientList.Find(x => x.id == messages[0] && x.ip == messages[1] && x.port == int.Parse(messages[2]));
                    if (baseClient != null)
                    {
                        long unixTime = GetNowUnixTime();
                        baseClient.lack = (unixTime - _beforeUnixTime) / 2;
                        long calcUnixTime = unixTime + baseClient.lack - 10;
                        TCPManagerServer.instance.SendWithAddress(CMDType.TimeSyncRecv, baseClient.ip, baseClient.port, "", calcUnixTime.ToString());
                        if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / Lack: {baseClient.lack} / senderId: {senderId}");
                    }
                    break;
                case CMDType.TimeSyncUI:
                    if (senderId != _id)
                    {
                        bool isShow = argument == "true";
                        if (isShow) TCPTimeSyncUI.instance?.Show(false);
                        else TCPTimeSyncUI.instance?.Hide(false);
                        TCPManagerServer.instance.Send(CMDType.TimeSyncUI, "[OTHERS]", message, argument, "");
                        if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / isShow: {isShow} / senderId: {senderId}");
                    }
                    break;
                case CMDType.TimeSyncMessage:
                    TimeSyncMessage timeSyncMessage = new TimeSyncMessage(messages[0], argument, long.Parse(messages[1]), senderId);
                    if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / message: {messages[0]} / argument: {argument} / senderId: {senderId}");
                    timeSyncMessages.Add(timeSyncMessage);
                    if (senderId != _id)
                    {
                        TCPManagerServer.instance.Send(CMDType.TimeSyncMessage, "[OTHERS]", message, argument, "");
                    }
                    break;
                case CMDType.Message: InvokeMessageReceivedEvent(receiverId, message, argument, senderId); break;
            }
        }

        private void OnClientMessageReceived(CMDType cmdType, string receiverId, string message, string argument, string senderId)
        {
            switch (cmdType)
            {
                case CMDType.ServerConnect:
                    TCPManagerClient.instance.serverBaseClient.Deserialize(message);
                    TCPManagerClient.instance.Send(CMDType.ServerConnectRecv, "", TCPManagerClient.instance.baseClient.Serialize());
                    ClientAdded?.Invoke(TCPManagerClient.instance.serverBaseClient.id);
                    PrintLog($"> {GetType().Name} / Client Added / clientId: {TCPManagerClient.instance.serverBaseClient.id}");
                    break;
                case CMDType.ServerDisconnect:
                    if (argument == "true" || !EnableAutoConnection) TCPManagerClient.instance.Disconnect();
                    if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / argument: {argument} / senderId: {senderId}");
                    break;
                case CMDType.TimeSyncRequest:
                    TCPManagerClient.instance.Send(CMDType.TimeSyncRequest, "", TCPManagerClient.instance.baseClient.Serialize());
                    break;
                case CMDType.TimeSyncRecv:
                    long unixTime = GetNowUnixTime();
                    long timeOffset = long.Parse(argument) - unixTime;
                    _unixTimeOffset = timeOffset;
                    TimeSyncRequested?.Invoke();
                    if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / Unix time offset: {_unixTimeOffset} / senderId: {senderId}");
                    break;
                case CMDType.TimeSyncUI:
                    if (senderId != _id)
                    {
                        bool isShow = argument == "true";
                        if (isShow) TCPTimeSyncUI.instance?.Show(false);
                        else TCPTimeSyncUI.instance?.Hide(false);
                        if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / isShow: {isShow} / senderId: {senderId}");
                    }
                    break;
                case CMDType.TimeSyncMessage:
                    string[] messages = message.Split(',');
                    TimeSyncMessage timeSyncMessage = new TimeSyncMessage(messages[0], argument, long.Parse(messages[1]), senderId);
                    timeSyncMessages.Add(timeSyncMessage);
                    if (_enableDetailedLog) PrintLog($"> {GetType().Name} / CMD Received: {cmdType} / message: {messages[0]} / argument: {argument} / senderId: {senderId}");
                    return;
                case CMDType.Message: InvokeMessageReceivedEvent(receiverId, message, argument, senderId); break;
            }
        }

        private void InvokeMessageReceivedEvent(string receiverId, string message, string argument, string senderId)
        {
            if (_enableMessageLog)
            {
                PrintLog($"> {GetType().Name} / Received / message: {message} / argument: {argument} / senderId: {senderId}");
            }
            MessageReceived?.Invoke(receiverId, message, argument, senderId);
        }

        private void InvokeMessageSentEvent(CMDType cmdType, string receiverId, string message, string argument, string senderId)
        {
            if (_enableMessageLog)
            {
                PrintLog($"> {GetType().Name} / Sent / cmdType: {cmdType} / receiverId: {receiverId} / message: {message} / argument: {argument} / senderId: {senderId}");
            }
            MessageSent?.Invoke(receiverId, message, argument, senderId);
        }

        private void OnClientRemoved(string clientId)
        {
            ClientRemoved?.Invoke(clientId);
            PrintLog($"> {GetType().Name} / Client Removed / clientId: {clientId}");
        }

        private void OnClientStatusUpdated()
        {
            ClientStatusUpdated?.Invoke();
        }

        private void LoadConfigData()
        {
            if (!File.Exists(_configDataPath))
            {
                _serverIp = NetworkUtility.GetLocalIPAddress();
                SaveConfigData();
            }
            else
            {
                try
                {
                    string[] rows = File.ReadAllLines(_configDataPath);

                    foreach (string row in rows)
                    {
                        string[] cols = row.Split(',');
                        switch (cols[0].ToLower())
                        {
                            case "isserver": _isServer = cols[1].ToLower() == "true"; break;
                            case "autoconnect": _enableAutoConnection = cols[1].ToLower() == "true"; break;
                            case "id": _id = cols[1]; break;
                            case "serverip": _serverIp = cols[1]; break;
                            case "serverport": int.TryParse(cols[1], out _serverPort); break;
                            case "timeoffset": int.TryParse(cols[1], out _timeOffset); break;
                            case "syncdelay": int.TryParse(cols[1], out _syncDelayMs); break;
                            case "syncinterval": int.TryParse(cols[1], out _syncInterval); break;
                        }
                    }
                }
                catch (Exception e)
                {
                    PrintLog("> " + GetType().Name + " / LoadConfigData / [Error] Read Fail: " + e.Message);
                }
            }
            ConfigUpdated?.Invoke();
        }

        private void SaveConfigData()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_configDataPath))
                {
                    writer.WriteLine($"IsServer,{_isServer}");
                    writer.WriteLine($"AutoConnect,{_enableAutoConnection}");
                    writer.WriteLine($"Id,{_id}");
                    writer.WriteLine($"ServerIp,{_serverIp}");
                    writer.WriteLine($"ServerPort,{_serverPort}");
                    writer.WriteLine($"TimeOffset,{_timeOffset}");
                    writer.WriteLine($"SyncDelay,{_syncDelayMs}");
                    writer.WriteLine($"SyncInterval,{_syncInterval}");
                }
            }
            catch (Exception e)
            {
                PrintLog("> " + GetType().Name + " / SaveConfigData / [Error] Write Fail: " + e.Message);
            }
            ConfigUpdated?.Invoke();
        }

        float lastTime;
        private void Update()
        {
            _globalUnixTime = GetGlobalUnixTime() + (_isServer ? 0 : _timeOffset);
            GlobalTimeUpdated?.Invoke();

            if (timeSyncMessages.Count > 0)
            {
                TimeSyncMessage timeSyncMessage = null;
                foreach (TimeSyncMessage message in timeSyncMessages)
                {
                    if (message.unixTime <= _globalUnixTime)
                    {
                        timeSyncMessage = message;
                        InvokeMessageReceivedEvent("[ALL]", timeSyncMessage.message, timeSyncMessage.argument, timeSyncMessage.senderId);
                        break;
                    }
                }
                if (timeSyncMessage != null) timeSyncMessages.Remove(timeSyncMessage);
            }

            if (_isServer)
            {
                lastTime += Time.deltaTime;
                if (lastTime > _syncInterval * 60f)
                {
                    lastTime = 0;
                    RequestTimeSync();
                }
            }
        }

        private long GetGlobalUnixTime()
        {
            return GetNowUnixTime() + _unixTimeOffset;
        }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            if (ApplicationDirectory.instance.isPrepared) Init();
        }

        private void OnEnable()
        {
            if (ApplicationDirectory.instance)
                ApplicationDirectory.instance.Prepared += Init;
            if (TCPManagerServer.instance)
            {
                TCPManagerServer.instance.StatusChanged += OnStatusChanged;
                TCPManagerServer.instance.ClientRemoved += OnClientRemoved;
                TCPManagerServer.instance.ClientStatusUpdated += OnClientStatusUpdated;
                TCPManagerServer.instance.MessageReceived += OnServerMessageReceived;
                TCPManagerServer.instance.MessageSent += InvokeMessageSentEvent;
            }
            if (TCPManagerClient.instance)
            {
                TCPManagerClient.instance.StatusChanged += OnStatusChanged;
                TCPManagerClient.instance.ServerDisconnected += OnClientRemoved;
                TCPManagerClient.instance.ServerStatusUpdated += OnClientStatusUpdated;
                TCPManagerClient.instance.MessageReceived += OnClientMessageReceived;
                TCPManagerClient.instance.MessageSent += InvokeMessageSentEvent;
            }
        }

        private void OnDisable()
        {
            if (ApplicationDirectory.instance)
                ApplicationDirectory.instance.Prepared -= Init;
            if (TCPManagerServer.instance)
            {
                TCPManagerServer.instance.StatusChanged -= OnStatusChanged;
                TCPManagerServer.instance.ClientRemoved -= OnClientRemoved;
                TCPManagerServer.instance.ClientStatusUpdated -= OnClientStatusUpdated;
                TCPManagerServer.instance.MessageReceived -= OnServerMessageReceived;
                TCPManagerServer.instance.MessageSent -= InvokeMessageSentEvent;
            }
            if (TCPManagerClient.instance)
            {
                TCPManagerClient.instance.StatusChanged -= OnStatusChanged;
                TCPManagerClient.instance.ServerDisconnected -= OnClientRemoved;
                TCPManagerClient.instance.ServerStatusUpdated -= OnClientStatusUpdated;
                TCPManagerClient.instance.MessageReceived -= OnClientMessageReceived;
                TCPManagerClient.instance.MessageSent -= InvokeMessageSentEvent;
            }
        }

        private long GetNowUnixTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void PrintLog(string log)
        {
            Debug.Log(log);
            TraceBox.Log(log);
        }
    }

}

