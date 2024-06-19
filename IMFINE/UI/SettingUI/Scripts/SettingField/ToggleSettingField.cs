namespace IMFINE.UI.SettingUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ToggleSettingField : SettingField, ISettingField
    {
        public delegate void ToggleSettingFieldEvent(string fieldID, bool value);
        public event ToggleSettingFieldEvent ValueChanged;

        [SerializeField]
        private bool _isOn = false;
        [SerializeField]
        private bool _isEditable = true;
        [Space]
        [SerializeField]
        private CanvasGroup _toggleButtonCG;
        [SerializeField]
        private Image _bg;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private ToggleButton _toggleButton;

        void Awake()
        {
            _toggleButton.SetDefaultValue(_isOn, true);
            _toggleButton.ValueChanged += OnToggleButtonValueChanged;
            SetEditable(_isEditable);
        }


        void OnDestroy()
        {
            _toggleButton.ValueChanged -= OnToggleButtonValueChanged;
        }





        private void OnToggleButtonValueChanged(bool isOn)
        {
            _isOn = isOn;
            ValueChanged?.Invoke(_id, _isOn);
        }

        public void SetEditable(bool isTrue)
        {
            _isEditable = isTrue;
            _toggleButtonCG.alpha = _isEditable ? 1 : .5f;
            _toggleButtonCG.blocksRaycasts = _isEditable;
        }





        public void Show()
        {
            _bg.gameObject.SetActive(true);
            _nameText.gameObject.SetActive(true);
            _toggleButton.Show();
        }

        public void Hide()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _toggleButton.Hide();
        }

        public void Reset()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _toggleButton.Reset();
        }

        public void SetValue(bool value)
        {
            _isOn = value;
            _toggleButton.SetDefaultValue(_isOn);
            ValueChanged?.Invoke(_id, _isOn);
        }

        public bool GetValue()
        {
            return _isOn;
        }

        public void SetName(string name)
        {
            _nameText.text = name;
        }
    }
}