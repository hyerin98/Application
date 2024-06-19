using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationEventExample : MonoBehaviour
{
    private void OnEnable()
    {
        ApplicationEvent.instance.backButtonClicked += OnClickAndroidBackButton;
        ApplicationEvent.instance.applicationFocusOff += OnApplicationFocusOff;
        ApplicationEvent.instance.applicationFocusOn += OnApplicationFocusOn;
        ApplicationEvent.instance.applicationPausedOff += OnApplicationPauseOff;
        ApplicationEvent.instance.applicationPausedOn += OnApplicationPauseOn;
    }

    private void OnDisable()
    {
        ApplicationEvent.instance.backButtonClicked -= OnClickAndroidBackButton;
        ApplicationEvent.instance.applicationFocusOff -= OnApplicationFocusOff;
        ApplicationEvent.instance.applicationFocusOn -= OnApplicationFocusOn;
        ApplicationEvent.instance.applicationPausedOff -= OnApplicationPauseOff;
        ApplicationEvent.instance.applicationPausedOn -= OnApplicationPauseOn;
    }

    private void OnClickAndroidBackButton()
    {
        Debug.Log("[Application Event] Android Back Button Clicked!");
        TraceBox.Log("[Application Event] Android Back Button Clicked!");
    }

    private void OnApplicationFocusOff()
    {
        Debug.Log("[Application Event] Application Focus Off!");
        TraceBox.Log("[Application Event] Application Focus Off!");
    }

    private void OnApplicationFocusOn()
    {
        Debug.Log("[Application Event] Application Focus On!");
        TraceBox.Log("[Application Event] Application Focus On!");
    }

    private void OnApplicationPauseOff()
    {
        Debug.Log("[Application Event] Application Pause Off!");
        TraceBox.Log("[Application Event] Application Pause Off!");
    }

    private void OnApplicationPauseOn()
    {
        Debug.Log("[Application Event] Application Pause On!");
        TraceBox.Log("[Application Event] Application Pause On!");
    }

}
