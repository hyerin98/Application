using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataFileLoader : MonoBehaviour
{
    [SerializeField] string dataFolderPath = "Video";
    [SerializeField] List<DataFile> dataFileList;

    public Action Prepared;
    private bool isPrepared = false;
    public List<DataFile> DataFileList { get => dataFileList; }
    public bool IsPrepared { get => isPrepared; }

    IEnumerator Start()
    {
        dataFileList = new List<DataFile>();
        yield return new WaitUntil(() => ApplicationDirectory.instance.isPrepared);
        string videoFolderPath = Path.Combine(ApplicationDirectory.instance.dataDirectoryInfo.FullName, dataFolderPath);

        if (!CheckDirectory(videoFolderPath)) yield break;
        foreach (FileInfo file in new DirectoryInfo(videoFolderPath).GetFiles())
        {
            dataFileList.Add(new DataFile { fileInfo = file, FileName = file.Name, FullPath = file.FullName, FileType = GetVideoType(file) });
        }
        isPrepared = true;
        Prepared?.Invoke();
    }

    private FileType GetVideoType(FileInfo file)
    {
        switch (file.Extension.ToLower())
        {
            case ".mp4":
            case ".avi":
            case ".mov":
            case ".mkv":
                return FileType.Video;
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".gif":
                return FileType.Image;
            case ".mp3":
            case ".wav":
            case ".ogg":
                return FileType.Audio;
            case ".txt":
                return FileType.Text;
            case ".json":
                return FileType.Json;
            case ".xml":
                return FileType.Xml;
            case ".csv":
                return FileType.Csv;
            default: return FileType.None;
        }
    }

    private bool CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            TraceBox.Log("> " + GetType().Name + " [Error] CheckDirectory / The directory does not exist : " + path);
            return false;
        }
        return true;
    }

    [Serializable]
    public struct DataFile
    {
        public FileInfo fileInfo;
        public string FileName;
        public string FullPath;
        public FileType FileType;
    }
}

public enum FileType { None, Image, Video, Audio, Text, Json, Xml, Csv }
