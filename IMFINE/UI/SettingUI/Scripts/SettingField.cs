namespace IMFINE.UI.SettingUI
{
    using UnityEngine;

    public class SettingField : MonoBehaviour
    {
        [SerializeField]
        protected string _id = "";

        public void SetID(string id)
        {
            _id = id;
        }

        public string GetID()
        {
            return _id;
        }
    }
}