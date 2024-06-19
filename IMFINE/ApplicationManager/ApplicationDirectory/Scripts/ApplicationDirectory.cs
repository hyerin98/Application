using System.Collections;
using System.Collections.Generic;
using System.IO;
using IMFINE.Utils;
using UnityEngine;

public class ApplicationDirectory : MonoSingleton<ApplicationDirectory>
{
#pragma warning disable CS0414
    public delegate void ApplicationDirectoryEvent();
    public event ApplicationDirectoryEvent Prepared;

    public enum AndroidStorageType { Internal, External };

    private bool _isPrepared = false;
    private DirectoryInfo _imfineDataDirectoryInfo = null;

    // #if UNITY_ANDROID
    [SerializeField]
    private AndroidStorageType _androidStorageType = AndroidStorageType.External;
    // #endif

    public bool isPrepared { get{ return _isPrepared; }}
    public DirectoryInfo dataDirectoryInfo
    { 
        get
        { 
            if(_imfineDataDirectoryInfo == null) SetStorageDirectory();
            return _imfineDataDirectoryInfo;
        }
    }

    private void CheckStorageDirectory(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
        {
            TraceBox.Log("> " + GetType().Name + " CheckStorageDirectory / The storage directory does not exist.");
            Debug.Log("> " + GetType().Name + " CheckStorageDirectory / The storage directory does not exist.");
            return;
        }
        else
        {
            TraceBox.Log("> " + GetType().Name + " CheckStorageDirectory / OK!");
            Debug.Log("> " + GetType().Name + " CheckStorageDirectory / OK!");
            return;
        }
    }

    void Awake()
    {
        // Android : /storage/emulated/0/imfine/[productName]/Data
        // Editor : [ProjectFolderName]/Data
        // PC : [buildFolderName]/Data
        // iOS : /var/mobile/Applications/프로그램ID/Documents

        SetStorageDirectory();

        if(!_imfineDataDirectoryInfo.Exists)
        {
            try
            {
                Directory.CreateDirectory(_imfineDataDirectoryInfo.FullName);
                TraceBox.Log(_imfineDataDirectoryInfo.Name + " Folder created");
                Debug.Log(_imfineDataDirectoryInfo.Name + " Folder created / " + _imfineDataDirectoryInfo.FullName);
            }
            catch(System.Exception e)
            {
                TraceBox.Log(e);
                Debug.Log(e);
            }
        }

        _isPrepared = true;
        Prepared?.Invoke();
    }

    private void SetStorageDirectory()
    {
        if(_imfineDataDirectoryInfo != null) return;

        #if UNITY_ANDROID && !UNITY_EDITOR

            DirectoryInfo storageDirectoryInfo;
            if(_androidStorageType == AndroidStorageType.Internal)
            {
                storageDirectoryInfo = new DirectoryInfo(Application.persistentDataPath);
            }
            else
            {
                storageDirectoryInfo = new DirectoryInfo(GetAndroidExternalPath());
            }

            Debug.Log("> " + GetType().Name + " / Android Storage Path: " + storageDirectoryInfo.FullName);
			
            CheckStorageDirectory(storageDirectoryInfo);

            string imfinePath = Path.Combine(storageDirectoryInfo.FullName, "imfine");
			DirectoryInfo imfineDirectoryInfo = new DirectoryInfo(imfinePath);
			if (!imfineDirectoryInfo.Exists)
			{
                try
                {
                    Directory.CreateDirectory(imfineDirectoryInfo.FullName);
                    TraceBox.Log(imfineDirectoryInfo.Name + " Folder created");
				    Debug.Log(imfineDirectoryInfo.Name + " Folder created / " + imfineDirectoryInfo.FullName);
                }
                catch(System.Exception e)
                {
                    TraceBox.Log(e);
                    Debug.Log(e);
                }
			}

            string productPath = Path.Combine(imfinePath, Application.productName.Replace(" ",""));
			DirectoryInfo productDirectoryInfo = new DirectoryInfo(productPath);
			if (!productDirectoryInfo.Exists)
			{
                try
                {
                    Directory.CreateDirectory(productDirectoryInfo.FullName);
                    TraceBox.Log(productDirectoryInfo.Name + " Folder created");
                    Debug.Log(productDirectoryInfo.Name + " Folder created / " + productDirectoryInfo.FullName);
                }
                catch(System.Exception e)
                {
                    TraceBox.Log(e);
                    Debug.Log(e);
                }
			}

            string dataPath = Path.Combine(productPath, "Data");
			_imfineDataDirectoryInfo = new DirectoryInfo(dataPath);
        
        #elif UNITY_IOS && !UNITY_EDITOR
            DirectoryInfo storageDirectoryInfo = new DirectoryInfo(Application.persistentDataPath);
            
            Debug.Log("> " + GetType().Name + " / iOS Storage Path: " + storageDirectoryInfo.FullName);

            CheckStorageDirectory(storageDirectoryInfo);

            _imfineDataDirectoryInfo = new DirectoryInfo(storageDirectoryInfo.FullName);
        
        #else
			DirectoryInfo dataDirectoryInfo = new DirectoryInfo(Application.dataPath);
			_imfineDataDirectoryInfo = new DirectoryInfo(dataDirectoryInfo.Parent.FullName + "/Data");
        #endif

        Debug.Log("> " + GetType().Name + " / imfineDataDirectoryInfo.FullName: " + _imfineDataDirectoryInfo.FullName);
        TraceBox.Log("> " + GetType().Name + " / imfineDataDirectoryInfo.FullName: " + _imfineDataDirectoryInfo.FullName);
    }

    public static string GetAndroidExternalPath()
    {
        string[] potentialDirectories = new string[]
        {
            "/sdcard",
            "/storage/emulated/0",
            "/mnt/sdcard",
            "/storage/sdcard0",
            "/storage/sdcard1"
        };

        if(Application.platform == RuntimePlatform.Android)
        {
            for(int i = 0; i < potentialDirectories.Length; i++)
            {
                if(Directory.Exists(potentialDirectories[i]))
                {
                    return potentialDirectories[i];
                }
            }
        }
        return "";
    }
}
