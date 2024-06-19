namespace IMFINE.Net.TCPManager
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public class TCPDeviceButton : MonoBehaviour
    {
        public delegate void DeviceButtonEvent(TCPDeviceButton deviceButton);
        public event DeviceButtonEvent Click;

        [SerializeField]
        private Text _idText;
        [SerializeField]
        private Text _ipPortText;
        [SerializeField]
        private Image _buttonImage;
        [SerializeField]
        private Toggle _checkToggle;
        [SerializeField]
        private Image _connectionStateImage;
        [SerializeField]
        private Color _connectColor;
        [SerializeField]
        private Color _disconnectColor;
        [SerializeField]
        private Color _connectingColor;


        private string _deviceID;
        private string _deviceIP;
        private int _devicePort;
        private bool _connected;

        public string deviceID
        {
            get { return _deviceID; }
        }
        public string deviceIP
        {
            get { return _deviceIP; }
        }
        public int devicePort
        {
            get { return _devicePort; }
        }
        public bool IsSelected
        {
            get { return _checkToggle.isOn; }
        }
        public bool Connected
        {
            get { return _connected; }
        }


        public void OnButtonClick()
        {
            Click?.Invoke(this);
        }

        public void SetData(string deviceID, string ip, int port)
        {
            _deviceID = deviceID;
            _deviceIP = ip;
            _devicePort = port;

            _idText.text = _deviceID;
            _ipPortText.text = _deviceIP + ":" + _devicePort;
        }

        public void SetConnectionState()
        {
            _connectionStateImage.color = _connectColor;
            _connected = true;
        }

        public void SetDisconnectionState()
        {
            _connectionStateImage.color = _disconnectColor;
            _connected = false;
        }

        public void SetConncectingState()
        {
            _connectionStateImage.color = _connectingColor;
        }

        public void Blink()
        {
            _buttonImage.color = _connectColor;
            DOTween.Kill("Color" + GetInstanceID());
            _buttonImage.DOColor(Color.white, 0.5f).SetId("Color" + GetInstanceID()).SetEase(Ease.InCubic);
        }

        private void OnDestroy()
        {
            DOTween.Kill("Color" + GetInstanceID());
        }
    }
}