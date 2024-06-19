namespace IMFINE.UI.SettingUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class HeaderSettingField : SettingField, ISettingField
    {
        [SerializeField]
        private Text _headerText;


        public void Show()
        {
            _headerText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _headerText.gameObject.SetActive(false);
        }

        public void Reset()
        {
            _headerText.gameObject.SetActive(false);
        }

        public void SetName(string name)
        {
            _headerText.text = name;
        }
    }
}