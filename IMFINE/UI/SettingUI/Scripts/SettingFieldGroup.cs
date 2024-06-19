namespace IMFINE.UI.SettingUI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SettingFieldGroup : MonoBehaviour
    {
        public delegate void SettingFieldGroupEvent();
        public event SettingFieldGroupEvent HeightChanged;
        public event SettingFieldGroupEvent Opened;
        public event SettingFieldGroupEvent Closed;

        [SerializeField]
        private string _id;
        [Space]
        [SerializeField]
        private SettingFieldGroupTitleBar _titleBar;
        [SerializeField]
        private RectTransform _fieldContainerRT;
        [SerializeField]
        private bool _isOpen = false;
        private List<GameObject> _fieldList = new List<GameObject>();
        private float _titleBarHeight = 80f;
        private float _fieldSpaceY = 15f;
        private float _bottomSpace = 100f;

        void Awake()
        {
            _titleBar.Changed += OnTitleBarChanged;

            for (int i = 0; i < _fieldContainerRT.childCount; i++)
            {
                SettingField settingField = _fieldContainerRT.GetChild(i).GetComponent<SettingField>();
                if (settingField != null)
                {
                    _fieldList.Add(_fieldContainerRT.GetChild(i).gameObject);
                }
            }
        }

        void Start()
        {
            _titleBar.isOpen = _isOpen;

            if (_isOpen) ShowSettingFields();
            else HideSettingFields();
        }

        void OnDestroy()
        {
            _titleBar.Changed -= OnTitleBarChanged;
        }






        private void OnTitleBarChanged(bool isOpen)
        {
            if (isOpen)
            {
                ShowSettingFields();
            }
            else
            {
                HideSettingFields();
            }
        }





        private void ShowSettingFields()
        {
            for (int i = 0; i < _fieldList.Count; i++)
            {
                _fieldList[i].SetActive(true);
            }
            Opened?.Invoke();
            UpdateHeight();
        }

        private void HideSettingFields()
        {
            for (int i = 0; i < _fieldList.Count; i++)
            {
                _fieldList[i].SetActive(false);
            }
            Closed?.Invoke();
            UpdateHeight();
        }


        public void UpdateHeight()
        {
            float _height = 0;
            foreach (GameObject field in _fieldList)
            {
                if (field.activeSelf)
                {
                    _height += ((RectTransform)field.transform).sizeDelta.y + _fieldSpaceY;
                }
            }
            Vector2 size = GetComponent<RectTransform>().sizeDelta;
            size.y = _height == 0 ? 100 : _height + _titleBarHeight + _bottomSpace;
            GetComponent<RectTransform>().sizeDelta = size;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>().parent.GetComponent<RectTransform>());
            HeightChanged?.Invoke();
        }


        public void AddField(GameObject settingFieldGO)
        {
            settingFieldGO.transform.SetParent(_fieldContainerRT);
            settingFieldGO.transform.localScale = Vector3.one;
            settingFieldGO.transform.localPosition = Vector3.zero;
            _fieldList.Add(settingFieldGO);

            if (_isOpen) ShowSettingFields();
            else HideSettingFields();

            HeightChanged?.Invoke();
        }

        public void RemoveField()
        {

        }

        public SettingField GetField(string fieldID)
        {
            for (int i = 0; i < _fieldList.Count; i++)
            {
                if (_fieldList[i].GetComponent<SettingField>().GetID().ToUpper() == fieldID.ToUpper())
                {
                    return _fieldList[i].GetComponent<SettingField>();
                }
            }

            return null;
        }

        public string GetID()
        {
            return _id;
        }
    }
}