namespace IMFINE.UI.SettingUI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SettingPanel : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _scrollViewContentRT;
        [SerializeField]
        private RectTransform _settingFieldGroupContainerRT;
        [SerializeField]
        private Text _applicationNameText;
        [SerializeField]
        private Text _versionText;
        private List<GameObject> _settingFieldGroupList = new List<GameObject>();
        private float _contentHeight = 0f;

        void Awake()
        {
            for(int i = 0; i < _settingFieldGroupContainerRT.childCount; i++)
            {
                RectTransform childRT = (RectTransform)_settingFieldGroupContainerRT.GetChild(i);
                if(childRT.GetComponent<SettingFieldGroup>() != null)
                {
                    childRT.GetComponent<SettingFieldGroup>().HeightChanged += OnSettingFieldGroupHeightChanged;
                    _settingFieldGroupList.Add(childRT.gameObject);
                }
            }

            UpdateContentHeight();
        }

        void Start()
        {
            _versionText.text = Application.version;
            _applicationNameText.text = Application.productName.ToUpper();
        }

        void OnDestroy()
        {
            for(int i = 0; i < _settingFieldGroupContainerRT.childCount; i++)
            {
                RectTransform childRT = (RectTransform)_settingFieldGroupContainerRT.GetChild(i);
                if(childRT.GetComponent<SettingFieldGroup>() != null)
                {
                    childRT.GetComponent<SettingFieldGroup>().HeightChanged -= OnSettingFieldGroupHeightChanged;
                }
            }
        }





        private void UpdateContentHeight()
        {
            _contentHeight = 0;
            for(int i = 0; i < _settingFieldGroupContainerRT.childCount; i++)
            {
                RectTransform childRT = (RectTransform)_settingFieldGroupContainerRT.GetChild(i);
                if(childRT.GetComponent<SettingFieldGroup>() != null)
                {
                    _contentHeight += childRT.sizeDelta.y;
                }
            }

            //Debug.Log("_contentHeight : " + _contentHeight);
            Vector2 scrollViewContentSize = _scrollViewContentRT.sizeDelta;
            scrollViewContentSize.y = _contentHeight;
            _scrollViewContentRT.sizeDelta = scrollViewContentSize;
        }





        private void OnSettingFieldGroupHeightChanged()
        {
            UpdateContentHeight();
        }





        public SettingFieldGroup GetGroup(string groupID)
        {
            for(int i = 0; i < _settingFieldGroupList.Count; i++)
            {
                if(_settingFieldGroupList[i].GetComponent<SettingFieldGroup>().GetID().ToUpper() == groupID.ToUpper())
                {
                    return _settingFieldGroupList[i].GetComponent<SettingFieldGroup>();
                }
            }

            return null;
        }
    }
}