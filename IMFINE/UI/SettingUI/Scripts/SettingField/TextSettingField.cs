namespace IMFINE.UI.SettingUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TextSettingField : SettingField, ISettingField
    {
        public delegate void TextSettingFieldEvent(string fieldID, string value);
        public event TextSettingFieldEvent ValueChanged;
        public event TextSettingFieldEvent ValueEditEnded;

        [SerializeField]
        private bool _isEditable = true;
        [SerializeField]
        private Image _bg;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private InputField _inputField;

        private Vector3 initPos;
        public bool IsEditable { get => _isEditable; }
        void Awake()
        {
            _inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            _inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            initPos = _inputField.GetComponent<RectTransform>().anchoredPosition;
            SetEditable(_isEditable);
        }


        void OnDestroy()
        {
            _inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
        }


        public void SetEditable(bool isTrue)
        {
            if (isTrue)
            {
                ((RectTransform)_inputField.transform).anchoredPosition = initPos;
                _inputField.interactable = true;
            }
            else
            {
                ((RectTransform)_inputField.transform).anchoredPosition = new Vector3(-20, 0, 0);
                _inputField.interactable = false;
            }
            _isEditable = isTrue;
        }



        private void OnInputFieldValueChanged(string changedValue)
        {
            _inputField.text = changedValue;
            ValueChanged?.Invoke(_id, changedValue);
        }

        private void OnInputFieldEndEdit(string changedValue)
        {
            _inputField.text = changedValue;
            ValueEditEnded?.Invoke(_id, changedValue);
        }



        public void Show()
        {
            _bg.gameObject.SetActive(true);
            _nameText.gameObject.SetActive(true);
            _inputField.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _inputField.gameObject.SetActive(false);
        }

        public void Reset()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _inputField.gameObject.SetActive(false);
        }

        public void SetName(string name)
        {
            _nameText.text = name.ToUpper();
        }

        public void SetValue(string value)
        {
            _inputField.text = value;
            ValueChanged?.Invoke(_id, value);
        }

        public string GetValue()
        {
            return _inputField.text;
        }
    }
}