
using DG.Tweening;
using IMFINE.Net.TCPManager;
using UnityEngine;
using UnityEngine.UI;

public class TCPManagerExample : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] Text numberText;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            TCPManager.instance.Send("ALL", "ALL WOW", "1111");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            TCPManager.instance.Send("OTHERS", "OTHERS WOW", "2222");
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            TCPManager.instance.Send("A", "A WOW", "3333");
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            TCPManager.instance.Send("B", "B WOW", "4444");
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            TCPManager.instance.Send("C", "B WOW", "4444");
        }
        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            SendTestTimeSyncMessage();
        }
    }

    private void ReceiveMessage(string receiverId, string message, string argument, string senderId)
    {
        string[] arguments = argument.Split(',');
        switch (message.ToUpper())
        {
            case "WOW": WOW(arguments[0]); break;
            case "TIMESYNC_TEST": ShowTimeSyncText(arguments[0]); break;
        }
    }

    int lastSyncNum = -1;
    private void SendTestTimeSyncMessage()
    {
        int syncNum;
        do
        {
            syncNum = Random.Range(0, 10);
        } while (syncNum == lastSyncNum);
        TCPManager.instance.SendTimeSync("TIMESYNC_TEST", syncNum.ToString());
    }


    private void WOW(string value)
    {
        TraceBox.Log("WOW! " + value);
    }

    void ShowTimeSyncText(string syncNum)
    {
        backgroundImage.color = Color.white;
        numberText.text = syncNum;
        DOTween.Kill("TCPTimeSyncUI" + GetInstanceID());
        DOVirtual.DelayedCall(3f, HideTimeSyncText).SetId("TCPTimeSyncUI" + GetInstanceID());
    }

    void HideTimeSyncText()
    {
        backgroundImage.color = Color.black;
        numberText.text = "";
    }

    void Start()
    {
        TCPManager.instance.MessageReceived += ReceiveMessage;
    }

}
