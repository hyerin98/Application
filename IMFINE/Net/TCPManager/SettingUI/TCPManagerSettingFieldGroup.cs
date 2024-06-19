namespace IMFINE.Net.TCPManager
{
    using System.Collections.Generic;
    using IMFINE.UI.SettingUI;
    using UnityEngine;
    using UnityEngine.UI;

    public class TCPManagerSettingFieldGroup : MonoBehaviour
    {
        [SerializeField] ToggleSettingField isServerField;
        [SerializeField] ToggleSettingField autoConnectField;
        [SerializeField] StringSettingField idField;
        [SerializeField] StringSettingField serverIpField;
        [SerializeField] IntSettingField serverPortField;
        [SerializeField] StringSettingField statusField;
        [SerializeField] Image connectionStateImage;
        [SerializeField] ButtonSettingField connectField;
        [SerializeField] HeaderSettingField connectClientsField;
        [SerializeField] ButtonSettingField disconnectSelectedClientsField;
        [SerializeField] ProgressButtonSettingField disconnectAllClientsField;
        [SerializeField] StringSettingField globalTimeField;
        [SerializeField] IntSettingField timeOffsetField;
        [SerializeField] IntSettingField syncDelayField;
        [SerializeField] IntSettingField syncIntervalField;
        [SerializeField] ButtonSettingField requestSyncField;

        [SerializeField] RectTransform clientListView;
        [SerializeField] RectTransform clientListContent;
        [SerializeField] SettingFieldGroup settingFieldGroup;
        [SerializeField] GameObject deviceButtonPrefab;

        [SerializeField] Color connectColor;
        [SerializeField] Color disconnectColor;
        [SerializeField] Color connectingColor;

        List<TCPDeviceButton> deviceButtons = new();

        public void OnClickConnectButton()
        {
            switch (TCPManager.instance.ETCPStatus)
            {
                case ETCPStatus.NotRunning:
                case ETCPStatus.Disconnected:
                    TCPManager.instance.StartConnect(TCPManager.instance.IsServer);
                    break;
                case ETCPStatus.Running:
                case ETCPStatus.Connected:
                case ETCPStatus.TryingConnect:
                case ETCPStatus.ConnectionError:
                    TCPManager.instance.Disconnect();
                    break;
            }
        }

        public void OnClickDisconnectSelectedClientsButton()
        {
            foreach (TCPDeviceButton deviceButton in deviceButtons)
            {
                if (deviceButton.IsSelected)
                {
                    TCPManagerServer.instance.DisconnectClientWithId(deviceButton.deviceID, true, false);
                }
            }
        }

        public void OnClickDisconnectAllClientsButton()
        {
            TCPManagerServer.instance.DisconnectAllClients(true, false);
        }

        public void OnClickShowTimeSyncUIButton()
        {
            TCPTimeSyncUI.instance?.Show();
        }

        public void OnClickRequestTimeSyncButton()
        {
            TCPManager.instance.RequestTimeSync();
        }


        private void UpdateConfigUI()
        {
            isServerField.SetValue(TCPManager.instance.IsServer);
            autoConnectField.SetValue(TCPManager.instance.EnableAutoConnection);
            idField.SetValue(TCPManager.instance.Id);
            serverIpField.SetValue(TCPManager.instance.ServerIp);
            serverIpField.SetEditable(!TCPManager.instance.IsServer);
            serverPortField.SetValue(TCPManager.instance.ServerPort);
            timeOffsetField.SetValue(TCPManager.instance.TimeOffset);
            syncDelayField.SetValue(TCPManager.instance.SyncDelayMs);
            syncIntervalField.SetValue(TCPManager.instance.SyncInterval);
            connectClientsField.SetName(TCPManager.instance.IsServer ? "CONNECTED CLIENTS ..0" : "CONNECTED SERVER");
            UpdateSettingFieldObjects();
        }

        private void UpdateStatusUI(ETCPStatus status)
        {
            bool isServer = TCPManager.instance.IsServer;
            bool isConnect = false;
            bool isEditable = false;
            switch (status)
            {
                case ETCPStatus.Running:
                case ETCPStatus.Connected:
                    connectField.SetName("DISCONNECT");
                    connectField.SetColor(disconnectColor);
                    connectionStateImage.color = connectColor;
                    isConnect = true;
                    isEditable = false;
                    break;
                case ETCPStatus.NotRunning:
                case ETCPStatus.Disconnected:
                    connectField.SetName("CONNECT");
                    connectField.SetColor(Color.white);
                    connectionStateImage.color = disconnectColor;
                    isConnect = false;
                    isEditable = true;
                    break;
                case ETCPStatus.TryingConnect:
                    connectField.SetName("STOP CONNECT");
                    connectField.SetColor(disconnectColor);
                    connectionStateImage.color = connectingColor;
                    isConnect = TCPManager.instance.IsServer;
                    isEditable = false;
                    break;
                case ETCPStatus.ConnectionError:
                    connectField.SetName("DISCONNECT");
                    connectField.SetColor(disconnectColor);
                    connectionStateImage.color = disconnectColor;
                    isConnect = true;
                    isEditable = false;
                    break;
            }

            isServerField.SetEditable(isEditable);
            autoConnectField.SetEditable(isEditable);
            idField.SetEditable(isEditable);
            serverIpField.SetEditable(isServer ? false : isEditable);
            serverPortField.SetEditable(isEditable);
            UpdateSettingFieldObjects();

            // Status
            string statusInfo = status.ToString();
            if (isConnect)
            {
                string id = idField.GetValue();
                string ip = NetworkUtility.GetLocalIPAddress();
                int port = isServer ? serverPortField.GetValue() : TCPManagerClient.instance.baseClient.port;
                statusInfo += $" | {ip}:{port}";
            }
            statusField.SetValue(statusInfo);

            // Clear clients button
            if (!isConnect && deviceButtons.Count > 0)
            {
                foreach (var deviceButton in deviceButtons)
                {
                    deviceButton.Click -= OnClickDeviceButton;
                    Destroy(deviceButton.gameObject);
                }
                deviceButtons.Clear();
            }
        }

        private void UpdateSettingFieldObjects()
        {
            bool _isRunning = TCPManager.instance.ETCPStatus.Equals(ETCPStatus.Running);
            disconnectSelectedClientsField.gameObject.SetActive(_isRunning);
            disconnectAllClientsField.gameObject.SetActive(_isRunning);
            syncIntervalField.gameObject.SetActive(_isRunning);
            requestSyncField.gameObject.SetActive(_isRunning);
            timeOffsetField.gameObject.SetActive(!TCPManager.instance.IsServer);
            settingFieldGroup.UpdateHeight();
        }

        private void UpdateClientListUI()
        {
            if (TCPManager.instance.IsServer)
            {
                foreach (BaseClient baseClient in TCPManagerServer.instance.BaseClientList.ToArray())
                {
                    UpdateDeviceButton(baseClient);
                }
            }
            else UpdateDeviceButton(TCPManagerClient.instance.serverBaseClient);

            int connectedCount = deviceButtons.Count;
            if (TCPManager.instance.IsServer)
            {
                connectClientsField.SetName($"CONNECTED CLIENTS ..{connectedCount}");
            }
            // Calculate Size Y
            float deviceButtonSizeY = deviceButtonPrefab.GetComponent<RectTransform>().sizeDelta.y;
            VerticalLayoutGroup verticalGroup = clientListContent.GetComponent<VerticalLayoutGroup>();
            int sizeCount = Mathf.Clamp(connectedCount, 1, 5);
            float finalSizeY = (deviceButtonSizeY * sizeCount) + (Mathf.Max(0, sizeCount - 1) * verticalGroup.spacing) + verticalGroup.padding.vertical;
            clientListView.sizeDelta = new Vector2(clientListView.sizeDelta.x, finalSizeY);
            settingFieldGroup.UpdateHeight();
        }

        private void OnClientAddedRemoved(string id)
        {
            UpdateClientListUI();
        }

        private void UpdateDeviceButton(BaseClient baseClient)
        {
            bool existId = !string.IsNullOrEmpty(baseClient.id);
            string id = existId ? baseClient.id : "";
            TCPDeviceButton deviceButton = deviceButtons.Find(x => x.deviceIP == baseClient.ip && existId && x.deviceID == id);

            if (deviceButton == null && existId)
            {
                deviceButton = Instantiate(deviceButtonPrefab, clientListContent).GetComponent<TCPDeviceButton>();
                deviceButton.SetData(id, baseClient.ip, baseClient.port);
                deviceButtons.Add(deviceButton);
                deviceButton.Click += OnClickDeviceButton;
            }

            if (deviceButton != null)
            {
                deviceButton.SetData(id, baseClient.ip, baseClient.port);
            }
            else return;

            if (baseClient.isOnline) deviceButton.SetConnectionState();
            else deviceButton.SetDisconnectionState();
        }

        private void UpdateGlobalTimeUI()
        {
            globalTimeField.SetValue(TCPManager.instance.GlobalUnixTime.ToString());
        }


        private void OnClickDeviceButton(TCPDeviceButton deviceButton)
        {
            if (TCPManager.instance.IsServer)
            {
                TCPManagerServer.instance.SendWithAddress(CMDType.Message, deviceButton.deviceIP, deviceButton.devicePort, "BLINK_DEVICE_BUTTON");
            }
            else TCPManagerClient.instance.Send(CMDType.Message, "", "BLINK_DEVICE_BUTTON");
        }

        private void OnIsServerValueChanged(string fieldID, bool value)
        {
            TCPManager.instance.ChangeIsServer(value);
        }

        private void OnAutoConnectValueChanged(string fieldID, bool value)
        {
            TCPManager.instance.ChangeEnableAutoConnection(value);
        }

        private void OnIdValueChanged(string fieldID, string value)
        {
            if (value.ToUpper() != value) idField.SetValue(value.ToUpper());
        }

        private void OnIdValueEditEnded(string fieldID, string value)
        {
            if (!TCPManager.instance.ChangeId(value)) idField.SetValue(TCPManager.instance.Id);
        }

        private void OnServerIpValueEditEnded(string fieldID, string value)
        {
            if (!TCPManager.instance.ChangeServerIp(value)) serverIpField.SetValue(TCPManager.instance.ServerIp);
        }

        private void OnServerPortValueEditEnded(string fieldID, int value)
        {
            if (!TCPManager.instance.ChangeServerPort(value)) serverPortField.SetValue(TCPManager.instance.ServerPort);
        }

        private void OnTimeOffsetValueEditEnded(string fieldID, int value)
        {
            if (!TCPManager.instance.ChangeTimeOffsetMs(value)) timeOffsetField.SetValue(TCPManager.instance.TimeOffset);
        }

        private void OnSyncDelayValueEditEnded(string fieldID, int value)
        {
            if (!TCPManager.instance.ChangeSyncDelayMs(value)) syncDelayField.SetValue(TCPManager.instance.SyncDelayMs);
        }

        private void OnSyncIntervalValueEditEnded(string fieldID, int value)
        {
            if (!TCPManager.instance.ChangePeriodMinutes(value)) syncIntervalField.SetValue(TCPManager.instance.SyncInterval);
        }

        private void Start()
        {
            if (TCPManager.instance.IsInitialized)
            {
                UpdateConfigUI();
                UpdateStatusUI(TCPManager.instance.ETCPStatus);
            }
        }

        private void OnEnable()
        {
            if (TCPManager.instance)
            {
                TCPManager.instance.Initialized += UpdateConfigUI;
                TCPManager.instance.ConfigUpdated += UpdateConfigUI;
                TCPManager.instance.StatusUpdated += UpdateStatusUI;
                TCPManager.instance.ClientAdded += OnClientAddedRemoved;
                TCPManager.instance.ClientRemoved += OnClientAddedRemoved;
                TCPManager.instance.ClientStatusUpdated += UpdateClientListUI;
                TCPManager.instance.GlobalTimeUpdated += UpdateGlobalTimeUI;
                TCPManager.instance.MessageReceived += OnMessageReceived;
            }
            isServerField.ValueChanged += OnIsServerValueChanged;
            autoConnectField.ValueChanged += OnAutoConnectValueChanged;
            idField.ValueChanged += OnIdValueChanged;
            idField.ValueEditEnded += OnIdValueEditEnded;
            serverIpField.ValueEditEnded += OnServerIpValueEditEnded;
            serverPortField.ValueEditEnded += OnServerPortValueEditEnded;
            timeOffsetField.ValueEditEnded += OnTimeOffsetValueEditEnded;
            syncDelayField.ValueEditEnded += OnSyncDelayValueEditEnded;
            syncIntervalField.ValueEditEnded += OnSyncIntervalValueEditEnded;
            settingFieldGroup.Opened += UpdateSettingFieldObjects;
        }

        private void OnMessageReceived(string receiverId, string message, string argument, string senderId)
        {
            if (message.Equals("BLINK_DEVICE_BUTTON"))
            {
                foreach (var deviceButton in deviceButtons)
                {
                    if (deviceButton.deviceID == senderId)
                    {
                        deviceButton.Blink();
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (TCPManager.instance)
            {
                TCPManager.instance.Initialized -= UpdateConfigUI;
                TCPManager.instance.ConfigUpdated -= UpdateConfigUI;
                TCPManager.instance.StatusUpdated -= UpdateStatusUI;
                TCPManager.instance.ClientAdded -= OnClientAddedRemoved;
                TCPManager.instance.ClientRemoved -= OnClientAddedRemoved;
                TCPManager.instance.ClientStatusUpdated -= UpdateClientListUI;
                TCPManager.instance.GlobalTimeUpdated -= UpdateGlobalTimeUI;
            }
            isServerField.ValueChanged -= OnIsServerValueChanged;
            autoConnectField.ValueChanged -= OnAutoConnectValueChanged;
            idField.ValueChanged -= OnIdValueChanged;
            idField.ValueEditEnded -= OnIdValueEditEnded;
            serverIpField.ValueEditEnded -= OnServerIpValueEditEnded;
            serverPortField.ValueEditEnded -= OnServerPortValueEditEnded;
            timeOffsetField.ValueEditEnded -= OnTimeOffsetValueEditEnded;
            syncDelayField.ValueEditEnded -= OnSyncDelayValueEditEnded;
            syncIntervalField.ValueEditEnded -= OnSyncIntervalValueEditEnded;
            settingFieldGroup.Opened -= UpdateSettingFieldObjects;
        }
    }
}