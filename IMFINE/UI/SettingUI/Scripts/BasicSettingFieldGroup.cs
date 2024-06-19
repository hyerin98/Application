namespace IMFINE.UI.SettingUI
{
    using UnityEngine;

	public class BasicSettingFieldGroup : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _fieldContainerRT;
		[SerializeField]
		private SettingFieldGroup _settingFieldGroup;
        [SerializeField]
        private ISettingData _settingData;

		void Start()
		{
            if(_settingData != null) CreateSettingField();
		}


        private string ToFieldName(string name)
        {
            string newName = "";
            for(int i = 0; i < name.Length; i++)
            {
                if(name[i].ToString() == name[i].ToString().ToUpper()) newName += " " + name[i];
                else newName += name[i];
            }

            return newName.ToUpper();
        }






		private void CreateSettingField()
		{
			int i = 0;
			foreach (var field in _settingData.GetType().GetFields())
			{
                string fieldName = field.FieldType.Name.ToLower();

                //Debug.Log("field.Name: " + field.Name + " / " + field.FieldType.Name);

                switch (fieldName)
                {
                    case "int32":
                        IntSettingField intSettingField = SettingPanelManager.instance.CreateIntSettingField();
                        intSettingField.SetName(ToFieldName(field.Name));
						intSettingField.SetValue((int)field.GetValue(_settingData));
						intSettingField.SetID(field.Name);
						intSettingField.ValueChanged += OnIntSettingFieldValueChanged;
                        _settingFieldGroup.AddField(intSettingField.gameObject);
                    break;
                    case "single":
                        IntSettingField intSettingField1 = SettingPanelManager.instance.CreateIntSettingField();
                        intSettingField1.SetName(ToFieldName(field.Name));
						intSettingField1.SetValue(int.Parse(field.GetValue(_settingData).ToString()));
						intSettingField1.SetID(field.Name);
						intSettingField1.ValueChanged += OnIntSettingFieldValueChanged;
                        _settingFieldGroup.AddField(intSettingField1.gameObject);
                    break;
                    case "float":
                        FloatSettingField floatSettingField = SettingPanelManager.instance.CreateFloatSettingField();
                        floatSettingField.SetName(ToFieldName(field.Name));
						floatSettingField.SetValue((int)field.GetValue(_settingData));
						floatSettingField.SetID(field.Name);
						floatSettingField.ValueChanged += OnFloatSettingFieldValueChanged;
                        _settingFieldGroup.AddField(floatSettingField.gameObject);
                    break;
                    case "string":
                        TextSettingField textSettingField = SettingPanelManager.instance.CreateTextSettingField();
                        textSettingField.SetName(ToFieldName(field.Name));
						textSettingField.SetValue((string)field.GetValue(_settingData));
						textSettingField.SetID(field.Name);
						textSettingField.ValueChanged += OnTextSettingFieldValueChanged;
                        _settingFieldGroup.AddField(textSettingField.gameObject);
                    break;
                    case "boolean":
                        ToggleSettingField toggleSettingField = SettingPanelManager.instance.CreateToggleSettingField();
                        toggleSettingField.SetName(ToFieldName(field.Name));
						toggleSettingField.SetValue((bool)field.GetValue(_settingData));
						toggleSettingField.SetID(field.Name);
						toggleSettingField.ValueChanged += OnToggleSettingFieldValueChanged;
                        _settingFieldGroup.AddField(toggleSettingField.gameObject);
                    break;
                    case "header":
                        HeaderSettingField headerSettingField = SettingPanelManager.instance.CreateHeaderSettingField();
                        headerSettingField.SetName(ToFieldName(field.Name));
						headerSettingField.SetID(field.Name);
                        _settingFieldGroup.AddField(headerSettingField.gameObject);
                    break;
                    case "linespace":
                    break;
                }

				i++;
			}
		}




		private void OnIntSettingFieldValueChanged(string fieldID, int value)
		{
			_settingData.GetType().GetField(fieldID).SetValue(_settingData, value);
		}

		private void OnFloatSettingFieldValueChanged(string fieldID, float value)
        {
            _settingData.GetType().GetField(fieldID).SetValue(_settingData, value);
        }

		private void OnTextSettingFieldValueChanged(string fieldID, string value)
        {
            _settingData.GetType().GetField(fieldID).SetValue(_settingData, value);
        }

		private void OnToggleSettingFieldValueChanged(string fieldID, bool value)
        {
            _settingData.GetType().GetField(fieldID).SetValue(_settingData, value);
        }
	}
}