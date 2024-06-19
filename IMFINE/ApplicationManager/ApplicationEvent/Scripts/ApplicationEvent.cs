using IMFINE.Utils;
using UnityEngine;

public class ApplicationEvent : MonoSingleton<ApplicationEvent>
{
    public delegate void applicationActionDelegate();
    public event applicationActionDelegate applicationFocusOn;
    public event applicationActionDelegate applicationFocusOff;
    public event applicationActionDelegate applicationPausedOn;
    public event applicationActionDelegate applicationPausedOff;

    public delegate void androidActionDelegate();
    public event androidActionDelegate backButtonClicked;

#if UNITY_ANDROID
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            backButtonClicked?.Invoke();
        }
    }
#endif

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) applicationFocusOn?.Invoke();
        else applicationFocusOff?.Invoke();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) applicationPausedOn?.Invoke();
        else applicationPausedOff?.Invoke();
    }
}
