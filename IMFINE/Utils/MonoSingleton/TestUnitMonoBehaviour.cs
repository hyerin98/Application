using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnitMonoBehaviour : MonoBehaviour
{
    [SerializeField]
    protected bool _isTestMode = false;
    [SerializeField]
    protected bool _enableDetailedLog = false;
    
    protected virtual void Awake() 
    {
        if(_isTestMode)    
        {
            Debug.LogWarning("> : " + GetType().Name + " / isTestMode: true");
        }
    }
}
