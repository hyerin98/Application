namespace IMFINE.Utils.ConfigManager
{
    using IMFINE.Utils;
    using System;
    using System.IO;
    using UnityEngine;

    public class ConfigManager : MonoSingleton<ConfigManager>
    {
        public delegate void ConfigManagerEvent();
        public event ConfigManagerEvent Prepared;
        public event ConfigManagerEvent FileSaved;

        private DirectoryInfo _dataDirectoryInfo;
        private string _configFilePath;
        private string _stringData;

        public ConfigData data;
        [NonSerialized]
        public bool isPrepared;

        private void Start()
        {
            SetupPath();
            ReadConfigFile();
            SaveConfigFile(false);

            isPrepared = true;
            Prepared?.Invoke();
        }

        public void SetupPath()
        {
            DirectoryInfo dataPathDirectoryInfo = new DirectoryInfo(ApplicationDirectory.instance.dataDirectoryInfo.FullName);
            _dataDirectoryInfo = new DirectoryInfo(dataPathDirectoryInfo.FullName);

            if (!_dataDirectoryInfo.Exists)
            {
                try
                {
                    Directory.CreateDirectory(_dataDirectoryInfo.FullName);
                    TraceBox.Log("> " + GetType().Name + " / Create 'Data' directroy");
                }
                catch (Exception e)
                {
                    TraceBox.Log("Error - Can not create 'Data' directory. " + e);
                }
            }
            _configFilePath = Path.Combine(_dataDirectoryInfo.FullName, "Config.json");
        }


        public void SaveConfigFile(bool sendEvent = true)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_configFilePath))
                {
                    writer.Write(JsonUtility.ToJson(data, true));
                }
                if (sendEvent)
                {
                    TraceBox.Log("> " + GetType().Name + " / Config file saved");
                    FileSaved?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.Log("> " + GetType().Name + " / SaveConfigFile / Error - " + e);
                TraceBox.Log("> " + GetType().Name + " / SaveConfigFile / Error - " + e);
            }
        }

        public void ReadConfigFile()
        {
            if (!File.Exists(_configFilePath))
            {
                data = new ConfigData();
                SaveConfigFile(false);
                TraceBox.Log("> " + GetType().Name + " / Create 'Config.json' file: " + _configFilePath);
            }
            try
            {
                _stringData = File.ReadAllText(_configFilePath);
                data = JsonUtility.FromJson<ConfigData>(_stringData);
                TraceBox.Log("> " + GetType().Name + " / Config file loaded");
            }
            catch (Exception e)
            {
                Debug.Log("> " + GetType().Name + " / ReadConfigFile / Error - " + e);
                TraceBox.Log("> " + GetType().Name + " / ReadConfigFile / Error - " + e);
            }
        }

        public void OpenConfigFile()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Debug.Log("> " + GetType().Name + " / OpenConfigFile / Config json file does not exist.");
                    return;
                }

#if UNITY_EDITOR_OSX
                    Application.OpenURL("file:///" + _configFilePath);
#elif UNITY_EDITOR_WIN
                Application.OpenURL(_configFilePath);
#endif

                Debug.Log("> " + GetType().Name + " / OpenConfigFile");
            }
            catch (Exception e)
            {
                Debug.Log("> " + GetType().Name + " / OpenConfigFile / Error - " + e);
            }
        }

        public void OpenConfigDirectory()
        {
            try
            {
                if (!_dataDirectoryInfo.Exists) Directory.CreateDirectory(_dataDirectoryInfo.FullName);
                Debug.Log("> " + GetType().Name + " / OpenConfigDirectory");

#if UNITY_EDITOR_OSX
                    Application.OpenURL("file:///" + _dataDirectoryInfo.FullName);
#elif UNITY_EDITOR_WIN
                Application.OpenURL(_dataDirectoryInfo.FullName);
#endif
            }
            catch (Exception e)
            {
                Debug.Log("> " + GetType().Name + " / OpenConfigDirectory / Error - " + e);
            }
        }

    }
}