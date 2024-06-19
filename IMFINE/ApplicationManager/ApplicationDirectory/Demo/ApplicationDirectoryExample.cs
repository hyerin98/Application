using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationDirectoryExample : MonoBehaviour
{
    void Awake()
    {
        Debug.Log(ApplicationDirectory.instance.dataDirectoryInfo.FullName);
        TraceBox.Log(ApplicationDirectory.instance.dataDirectoryInfo.FullName);

        ApplicationDirectory.instance.Prepared += OnApplicationDirectoryPrepared;
        if(ApplicationDirectory.instance.isPrepared) OnApplicationDirectoryPrepared();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        ApplicationDirectory.instance.Prepared -= OnApplicationDirectoryPrepared;
    }





    private void OnApplicationDirectoryPrepared()
    {
        Debug.Log("Application directory prepared");
        TraceBox.Log("Application directory prepared");
    }
}
