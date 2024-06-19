namespace IMFINE.Net.TCPManager
{
    using System;
    using IMFINE.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    public class TCPTimeSyncUI : MonoSingleton<TCPTimeSyncUI>
    {
        [SerializeField] bool showOnStart = false;
        [SerializeField] bool enableKeyOpen = true;
        [SerializeField] GameObject timeSyncObject;

        [SerializeField] AudioSource effectSound;
        [SerializeField] Image backgroundImage;
        [SerializeField] Text numberText;
        [SerializeField] Text eventText;

        bool isShow;
        static double old_seconds = 0;


        public void Show(bool sendEvent = true)
        {
            if (isShow) return;
            isShow = true;
            timeSyncObject.SetActive(true);
            if (sendEvent) TCPManager.instance.RequestTimeSyncUI(true);
        }

        public void Hide(bool sendEvent = true)
        {
            if (!isShow) return;
            isShow = false;
            timeSyncObject.SetActive(false);
            if (sendEvent) TCPManager.instance.RequestTimeSyncUI(false);
        }

        void Awake()
        {
            if (showOnStart) Show();
            else Hide();
        }

        void Update()
        {
            if (enableKeyOpen)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        if (!isShow) Show();
                        else Hide();
                    }
                }
            }

            if (!isShow) return;

            double now = TCPManager.instance.GlobalUnixTime;
            //tx_now.Text = now.ToString(); 현재 시간 텍스트 설정
            double now_sec = Math.Ceiling(now / 1000);


            //1초마다 텍스트를 변경하거나 시간이 바뀔 때만 실행됨
            if (old_seconds != now_sec)
            {
                //초 단위로 변경된 시간의 마지막 숫자를 가져옴
                var last = now_sec.ToString()[now_sec.ToString().Length - 1];
                numberText.text = last.ToString();

                //짝수 초일 때 흰색, 홀수 초일 때 검은색으로 배경색 변경
                if (now_sec % 2 == 0)
                {
                    backgroundImage.color = Color.white;
                }
                else
                {
                    backgroundImage.color = Color.black;
                }
                old_seconds = now_sec;
                effectSound.Play();
            }
        }
    }
}
