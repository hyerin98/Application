namespace IMFINE.Utils.ConfigManager
{
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(ConfigManager))]
	public class ConfigManagerEditor : Editor
	{
		private ConfigManager configManager;
		private GUIStyle _helpTextStyle;

		public void OnEnable()
		{
			configManager = (ConfigManager)target;
			configManager.SetupPath();

			Color helpTextColor;
			ColorUtility.TryParseHtmlString("#CBB99D", out helpTextColor);
			_helpTextStyle = new GUIStyle();
			_helpTextStyle.normal.textColor = helpTextColor;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();
			if (GUILayout.Button("Update JSON"))
			{
				configManager.ReadConfigFile();
				configManager.SaveConfigFile();
				Debug.Log("> " + GetType().Name + " / Config josn file updated");
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Open Config File Folder")) configManager.OpenConfigDirectory();
			if (GUILayout.Button("Open Config File")) configManager.OpenConfigFile();
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
		}
	}
}