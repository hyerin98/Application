namespace IMFINE.Application
{
#pragma warning disable CS0414
    using IMFINE.Utils;
    using UnityEngine;

    public class ApplicationManager : MonoSingleton<ApplicationManager>
    {
        public enum FPS { FPS60, FPS30 };

        [SerializeField]
        private bool _enableScreenNeverSleep = true;
        [SerializeField]
        private bool _hideMouseOnStart = true;
        [SerializeField]
        private FPS _targetFPS = FPS.FPS60;

        void Awake()
        {
#if UNITY_ANDROID
            if (_enableScreenNeverSleep)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
#endif
#if UNITY_STANDALONE && !UNITY_EDITOR
            if(_hideMouseOnStart) Cursor.visible = false;
#endif

            if (_targetFPS == FPS.FPS30) Application.targetFrameRate = 30;
            else if (_targetFPS == FPS.FPS60) Application.targetFrameRate = 60;
        }

#if UNITY_STANDALONE && !UNITY_EDITOR
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    Application.Quit();
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    if (Cursor.visible) Cursor.visible = false;
                    else Cursor.visible = true;
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    try
                    {
                        Application.OpenURL(Application.consoleLogPath);
                    }
                    catch
                    {
                    }
                }
            }
        }
#endif

        public void Restart()
        {
#if UNITY_ANDROID
            AndroidJavaObject AOSUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaClass PendingIntentClass = new AndroidJavaClass("android.app.PendingIntent");
            AndroidJavaObject baseContext = AOSUnityActivity.Call<AndroidJavaObject>("getBaseContext");
            AndroidJavaObject intentObj
             = baseContext.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", baseContext.Call<string>("getPackageName"));

            AndroidJavaObject context = AOSUnityActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject pendingIntentObj
            = PendingIntentClass.CallStatic<AndroidJavaObject>("getActivity", context, 123456, intentObj, PendingIntentClass.GetStatic<int>("FLAG_CANCEL_CURRENT"));

            AndroidJavaClass AlarmManagerClass = new AndroidJavaClass("android.app.AlarmManager");
            AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System");

            AndroidJavaObject mAlarmManager = AOSUnityActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
            long restartMillis = JavaSystemClass.CallStatic<long>("currentTimeMillis") + 100;
            mAlarmManager.Call("set", AlarmManagerClass.GetStatic<int>("RTC"), restartMillis, pendingIntentObj);

            JavaSystemClass.CallStatic("exit", 0);
#endif
        }
    }
}