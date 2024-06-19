namespace IMFINE.UI.SettingUI
{
    using IMFINE.Utils;
    using UnityEngine;

    public class SettingPanelManager : MonoSingleton<SettingPanelManager>
    {
        public enum SettingFieldType { BUTTON, FLOAT, INT, HEADER, SPACE, TEXT, TOGGLE };

        [SerializeField]
        private GameObject _buttonSettingField;
        [SerializeField]
        private GameObject _floatSettingField;
        [SerializeField]
        private GameObject _headerSettingField;
        [SerializeField]
        private GameObject _intSettingField;
        [SerializeField]
        private GameObject _spaceSettingField;
        [SerializeField]
        private GameObject _textSettingField;
        [SerializeField]
        private GameObject _toggleSettingField;
        [SerializeField]
        private SettingPanel _settingPanel;


        public ButtonSettingField CreateButtonSettingField()
        {
            GameObject instance = Instantiate(_buttonSettingField);
            return instance.GetComponent<ButtonSettingField>();
        }

        public FloatSettingField CreateFloatSettingField()
        {
            GameObject instance = Instantiate(_floatSettingField);
            return instance.GetComponent<FloatSettingField>();
        }

        public HeaderSettingField CreateHeaderSettingField()
        {
            GameObject instance = Instantiate(_headerSettingField);
            HeaderSettingField settingField = instance.GetComponent<HeaderSettingField>();
            return settingField;
        }

        public IntSettingField CreateIntSettingField()
        {
            GameObject instance = Instantiate(_intSettingField);
            return instance.GetComponent<IntSettingField>();
        }

        public SpaceSettingField CreateSpaceSettingField()
        {
            GameObject instance = Instantiate(_spaceSettingField);
            return instance.GetComponent<SpaceSettingField>();
        }

        public TextSettingField CreateTextSettingField()
        {
            GameObject instance = Instantiate(_textSettingField);
            return instance.GetComponent<TextSettingField>();
        }

        public ToggleSettingField CreateToggleSettingField()
        {
            GameObject instance = Instantiate(_toggleSettingField);
            return instance.GetComponent<ToggleSettingField>();
        }




        public SettingFieldGroup GetGroup(string groupID)
        {
            return _settingPanel.GetGroup(groupID);
        }
    }
}