namespace IMFINE.Utils.ConfigManager
{
	using UnityEngine;
	using IMFINE.Debugs;
	using IMFINE.UI.SettingUI;
	using System;

	public class ConfigSettingFieldGroup : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _fieldContainerRT;
		[SerializeField]
		private SettingFieldGroup _settingFieldGroup;
		[SerializeField]
		private bool _enableAutoUpdates = true;


		void Awake()
		{
			ConfigManager.instance.Prepared += OnConfigDataPrepared;
			ConfigManager.instance.FileSaved += OnConfigFileSaved;
		}

		void OnDestroy()
		{
			if (ConfigManager.instance != null)
			{
				ConfigManager.instance.Prepared -= OnConfigDataPrepared;
				ConfigManager.instance.FileSaved -= OnConfigFileSaved;
			}
		}

		private void Start()
		{
			if (ConfigManager.instance.isPrepared) OnConfigDataPrepared();
		}





		private string ToFieldName(string name)
		{
			string newName = "";
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i].ToString() == name[i].ToString().ToUpper()) newName += " " + name[i];
				else newName += name[i];
			}

			return newName.ToUpper();
		}





		bool isSetConfigData;
		private void OnConfigDataPrepared()
		{
			if (isSetConfigData) return;
			try
			{
				int i = 0;
				foreach (var field in ConfigManager.instance.data.GetType().GetFields())
				{
					string fieldName = field.FieldType.Name.ToLower();
					switch (fieldName)
					{
						case "int32":
							IntSettingField intSettingField = SettingPanelManager.instance.CreateIntSettingField();
							intSettingField.SetName(ToFieldName(field.Name));
							intSettingField.SetValue((int)field.GetValue(ConfigManager.instance.data));
							intSettingField.SetID(field.Name);
							intSettingField.ValueChanged += OnIntSettingFieldValueChanged;
							_settingFieldGroup.AddField(intSettingField.gameObject);
							break;
						case "single":
							FloatSettingField floatSettingField = SettingPanelManager.instance.CreateFloatSettingField();
							floatSettingField.SetName(ToFieldName(field.Name));
							floatSettingField.SetValue((float)field.GetValue(ConfigManager.instance.data));
							floatSettingField.SetID(field.Name);
							floatSettingField.ValueChanged += OnFloatSettingFieldValueChanged;
							_settingFieldGroup.AddField(floatSettingField.gameObject);
							break;
						case "float":
							FloatSettingField floatSettingField1 = SettingPanelManager.instance.CreateFloatSettingField();
							floatSettingField1.SetName(ToFieldName(field.Name));
							floatSettingField1.SetValue((float)field.GetValue(ConfigManager.instance.data));
							floatSettingField1.SetID(field.Name);
							floatSettingField1.ValueChanged += OnFloatSettingFieldValueChanged;
							_settingFieldGroup.AddField(floatSettingField1.gameObject);
							break;
						case "string":
							TextSettingField textSettingField = SettingPanelManager.instance.CreateTextSettingField();
							textSettingField.SetName(ToFieldName(field.Name));
							textSettingField.SetValue((string)field.GetValue(ConfigManager.instance.data));
							textSettingField.SetID(field.Name);
							textSettingField.ValueChanged += OnTextSettingFieldValueChanged;
							_settingFieldGroup.AddField(textSettingField.gameObject);
							break;
						case "boolean":
							ToggleSettingField toggleSettingField = SettingPanelManager.instance.CreateToggleSettingField();
							toggleSettingField.SetName(ToFieldName(field.Name));
							toggleSettingField.SetValue((bool)field.GetValue(ConfigManager.instance.data));
							toggleSettingField.SetID(field.Name);
							toggleSettingField.ValueChanged += OnToggleSettingFieldValueChanged;
							_settingFieldGroup.AddField(toggleSettingField.gameObject);
							break;
					}

					i++;
				}
				isSetConfigData = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("> " + GetType().Name + " / OnConfigDataPrepared / ex: " + ex);
			}
		}

		public void UpdateData()
		{
			try
			{
				int i = 0;
				foreach (var field in ConfigManager.instance.data.GetType().GetFields())
				{
					string fieldName = field.FieldType.Name.ToLower();

					switch (fieldName)
					{
						case "int32":
							IntSettingField intSettingField = (IntSettingField)_settingFieldGroup.GetField(field.Name);
							intSettingField.SetValue(Convert.ToInt32(field.GetValue(ConfigManager.instance.data)));
							break;
						case "single":
							FloatSettingField floatSettingField = (FloatSettingField)_settingFieldGroup.GetField(field.Name);
							floatSettingField.SetValue(Convert.ToSingle(field.GetValue(ConfigManager.instance.data)));
							break;
						case "float":
							FloatSettingField floatSettingField1 = (FloatSettingField)_settingFieldGroup.GetField(field.Name);
							floatSettingField1.SetValue(Convert.ToSingle(field.GetValue(ConfigManager.instance.data)));
							break;
						case "string":
							TextSettingField textSettingField = (TextSettingField)_settingFieldGroup.GetField(field.Name);
							textSettingField.SetValue(Convert.ToString(field.GetValue(ConfigManager.instance.data)));
							break;
						case "boolean":
							ToggleSettingField toggleSettingField = (ToggleSettingField)_settingFieldGroup.GetField(field.Name);
							toggleSettingField.SetValue(Convert.ToBoolean(field.GetValue(ConfigManager.instance.data)));
							break;
					}

					i++;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("> " + GetType().Name + " / UpdateData / ex: " + ex);
			}
		}

		private void OnConfigFileSaved()
		{
			if (_enableAutoUpdates) UpdateData();
		}





		private void OnIntSettingFieldValueChanged(string fieldID, int value)
		{
			ConfigManager.instance.data.GetType().GetField(fieldID).SetValue(ConfigManager.instance.data, value);
		}

		private void OnFloatSettingFieldValueChanged(string fieldID, float value)
		{
			ConfigManager.instance.data.GetType().GetField(fieldID).SetValue(ConfigManager.instance.data, value);
		}

		private void OnTextSettingFieldValueChanged(string fieldID, string value)
		{
			ConfigManager.instance.data.GetType().GetField(fieldID).SetValue(ConfigManager.instance.data, value);
		}

		private void OnToggleSettingFieldValueChanged(string fieldID, bool value)
		{
			ConfigManager.instance.data.GetType().GetField(fieldID).SetValue(ConfigManager.instance.data, value);
		}

		public void OnOpenConfigFileButtonPress()
		{
			ConfigManager.instance.OpenConfigFile();
		}

		public void OnOpenConfigDirectoryButtonPress()
		{
			ConfigManager.instance.OpenConfigDirectory();
		}

		public void OnApplyButtonClick()
		{
			ConfigManager.instance.SaveConfigFile();
			Toast.instance.Bake("Config File Saved!");
		}
	}
}