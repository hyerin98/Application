using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IMFINE.UI.SettingUI
{
    public class ButtonSettingField : SettingField, ISettingField
    {
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private Button _button;



        public void SetName(string name)
        {
            if (_nameText != null)
                _nameText.text = name;
        }
        
        public void SetColor(Color color)
        {
            if (_button != null)
                _button.image.color = color;
        }

        public void AddListener(UnityAction action) 
        {
            if (_button != null)
                _button.onClick.AddListener(action);
        }

        public void RemoveListener() 
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }

        
        public void Show()
        {
            throw new System.NotImplementedException();
        }

        public void Hide()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }   
}