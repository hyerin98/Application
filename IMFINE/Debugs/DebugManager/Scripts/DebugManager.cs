namespace IMFINE.Debugs
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class DebugManager : MonoBehaviour
    {
        public delegate void DebugManagerEvent();
        public event DebugManagerEvent ShowEvent;
        public event DebugManagerEvent HideEvent;
        public event DebugManagerEvent HoldEvent;
        public event DebugManagerEvent TestModeEvent;

        [Header("Basic Settings")]
        public Transform DebugPanel;
        public bool showPanelOnStart = false;
        public bool hideDebugButton = true;

        [Header("Click")]
        [Range(1, 8)]
        public int ClickCountToOpen = 1;
        [Range(0, 3)]
        public float WaitTimeToClosable = 1;
        readonly float clickTimeOutSec = 1;

        [Header("Hold")]
        [Range(1, 5)]
        public float WaitTimeToHoldEvent = 3;

        [Header("Test")]
        public Button TestModeButton;
        [Range(1, 10)]
        public int ClickCountToTest = 1;
        readonly float testTimeOutSec = 1.5f;

        [Header("Key")]
        public bool enableKeyOpen = true;
        public bool openWithShiftKey = true;
        public KeyCode OpenCode = KeyCode.D;

        private bool isDebugMode = false;
        private bool startClickTimer = false;
        private float click_pastTime = 0;
        private int clickCount = 0;

        private bool startTestTimer = false;
        private float test_pastTime = 0;
        private int testCount = 0;

        private bool isHold = false;
        private bool startHoldTimer = false;
        private float hold_pastTime = 0;
        private float wait_pastTime = 0;

        private CanvasGroup DebugPanelCG;

        public bool IsDebugMode { get => isDebugMode; }

        void Awake()
        {
            if (hideDebugButton) GetComponent<Image>().color = Color.clear;
            if (DebugPanel)
            {
                if (!DebugPanel.gameObject.activeInHierarchy)
                    DebugPanel.gameObject.SetActive(true);
                if (DebugPanel.GetComponent<CanvasGroup>()) DebugPanelCG = DebugPanel.GetComponent<CanvasGroup>();
                else DebugPanelCG = DebugPanel.gameObject.AddComponent<CanvasGroup>();
                SetPanelActive(showPanelOnStart);
            }
            if (TestModeButton) TestModeButton.onClick.AddListener(OnClickTestButton);
        }

        void Update()
        {
            if (enableKeyOpen)
            {

                if (openWithShiftKey)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetKeyDown(OpenCode))
                        {
                            if (!(EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>()))
                            {
                                EventSystem.current.SetSelectedGameObject(null);
                                SetPanelActive(!IsDebugMode);
                            }
                        }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(OpenCode))
                    {
                        if (!(EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>()))
                        {
                            EventSystem.current.SetSelectedGameObject(null);
                            SetPanelActive(!IsDebugMode);
                        }
                    }
                }
            }

            if (wait_pastTime > 0)
            {
                wait_pastTime -= Time.deltaTime;
            }

            if (startClickTimer)
            {
                click_pastTime += Time.deltaTime;
                if (clickCount >= ClickCountToOpen) SetPanelActive(true);
                if (click_pastTime >= clickTimeOutSec) ResetClick();
            }
            if (startTestTimer)
            {
                test_pastTime += Time.deltaTime;
                if (testCount >= ClickCountToTest) { TestModeEvent?.Invoke(); ResetTest(); }
                if (test_pastTime >= testTimeOutSec) ResetTest();
            }
            if (startHoldTimer)
            {
                hold_pastTime += Time.deltaTime;
                if (hold_pastTime >= WaitTimeToHoldEvent) OnHoldButton();
            }
        }

        public void OnPointerDownDebug()
        {
            if (!startHoldTimer) startHoldTimer = true;
        }

        public void OnPointerUpDebug()
        {
            if (isHold) { isHold = false; return; }
            if (!IsDebugMode)
            {
                clickCount++;
                if (!startClickTimer) startClickTimer = true;
            }
            else
            {
                if (wait_pastTime <= 0)
                    SetPanelActive(false);
            }
        }

        public void OnHoldButton()
        {
            isHold = true;
            ResetHold();
            HoldEvent?.Invoke();
            //Debug.Log("Hold Event Occur!");
        }

        public void OnClickTestButton()
        {
            testCount++;
            if (!startTestTimer) startTestTimer = true;
        }

        public void SetPanelActive(bool isActive)
        {
            isDebugMode = isActive;
            ResetClick();

            if (isActive)
            {
                DebugPanelCG.alpha = 1;
                DebugPanelCG.interactable = true;
                DebugPanelCG.blocksRaycasts = true;
                ShowEvent?.Invoke();
                wait_pastTime = WaitTimeToClosable;
                //Debug.Log("Debug Panel Open!");
            }
            else
            {
                DebugPanelCG.alpha = 0;
                DebugPanelCG.interactable = false;
                DebugPanelCG.blocksRaycasts = false;
                HideEvent?.Invoke();
                //Debug.Log("Debug Panel Close!");
            }
        }

        public void ResetClick()
        {
            clickCount = 0;
            startClickTimer = false;
            click_pastTime = 0;
        }
        public void ResetTest()
        {
            testCount = 0;
            startTestTimer = false;
            test_pastTime = 0;
        }
        public void ResetHold()
        {
            startHoldTimer = false;
            hold_pastTime = 0;
        }
    }
}
