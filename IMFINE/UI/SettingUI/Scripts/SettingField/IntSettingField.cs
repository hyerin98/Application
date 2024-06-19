namespace IMFINE.UI.SettingUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class IntSettingField : SettingField, ISettingField
    {
        public delegate void NumberSettingFieldEvent(string fieldID, int value);
        public event NumberSettingFieldEvent ValueChanged;
        public event NumberSettingFieldEvent ValueEditEnded;

        [SerializeField]
        private int _offset = 1;
        [SerializeField]
        private bool _isEditable = true;
        [SerializeField]
        private Image _bg;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private InputField _inputField;
        [SerializeField]
        private Button _minusButton;
        [SerializeField]
        private Button _plusButton;
        private int _value = 0;

        private Vector3 initPos;
        public bool IsEditable { get => _isEditable; }

        void Awake()
        {
            initPos = _inputField.GetComponent<RectTransform>().anchoredPosition;
            _inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            _inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            _minusButton.onClick.AddListener(OnMinusButtonClick);
            _plusButton.onClick.AddListener(OnPlusButtonClick);
            _inputField.text = _value.ToString();
            SetEditable(_isEditable);
        }

        void OnDestroy()
        {
            _inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            _minusButton.onClick.RemoveListener(OnMinusButtonClick);
            _plusButton.onClick.RemoveListener(OnPlusButtonClick);
        }


        public void SetEditable(bool isTrue)
        {
            if (isTrue)
            {
                _minusButton.gameObject.SetActive(true);
                _plusButton.gameObject.SetActive(true);
                _inputField.GetComponent<RectTransform>().anchoredPosition = initPos;
                _inputField.interactable = true;
            }
            else
            {
                _minusButton.gameObject.SetActive(false);
                _plusButton.gameObject.SetActive(false);
                _inputField.GetComponent<RectTransform>().anchoredPosition = initPos + new Vector3(205, 0, 0);
                _inputField.interactable = false;
            }
            _isEditable = isTrue;
        }


        private void OnInputFieldValueChanged(string value)
        {
            _value = int.Parse(value);
            ValueChanged?.Invoke(_id, _value);
        }

        private void OnInputFieldEndEdit(string value)
        {
            _value = int.Parse(value);
            ValueEditEnded?.Invoke(_id, _value);
        }

        private void OnMinusButtonClick()
        {
            _value -= _offset;
            _inputField.text = _value.ToString();
            ValueEditEnded?.Invoke(_id, _value);
        }

        private void OnPlusButtonClick()
        {
            _value += _offset;
            _inputField.text = _value.ToString();
            ValueEditEnded?.Invoke(_id, _value);
        }




        public void Show()
        {
            _bg.gameObject.SetActive(true);
            _nameText.gameObject.SetActive(true);
            _inputField.gameObject.SetActive(true);

            if (_isEditable)
            {
                _minusButton.gameObject.SetActive(true);
                _plusButton.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _inputField.gameObject.SetActive(false);
            _minusButton.gameObject.SetActive(false);
            _plusButton.gameObject.SetActive(false);
        }

        public void Reset()
        {
            _bg.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(false);
            _inputField.gameObject.SetActive(false);
            _minusButton.gameObject.SetActive(false);
            _plusButton.gameObject.SetActive(false);
        }

        public void SetName(string name)
        {
            _nameText.text = name;
        }

        public void SetOffset(int offset)
        {
            _offset = offset;
        }

        public void SetValue(int value)
        {
            _value = value;
            _inputField.text = _value.ToString();
            ValueChanged?.Invoke(_id, _value);
        }

        public int GetValue()
        {
            return _value;
        }
    }
}