using System.Collections;
using System.Collections.Generic;
using IMFINE.Application;
using UnityEngine;

public class ApplicationManagerExample : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public void OnRestartButtonClick()
    {
        ApplicationManager.instance.Restart();
    }
}
