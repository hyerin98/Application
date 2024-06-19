using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using IMFINE.Utils;

public class TraceBox : MonoSingleton<TraceBox>
{
#pragma warning disable CS0414
    [Header("Set Properties")]
    [SerializeField] bool showDebugLog = false;
    public bool ShowDebugLog { get => showDebugLog; }
    [SerializeField] bool autoUpdateScroll = true;
    [SerializeField] private int limitLineNum = 50;

    [Header("UI Properties")]
    [SerializeField]
    private Text _text;
    [SerializeField]
    private Text _versionText;
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private GameObject _container;
    private StringBuilder _sb;
    private int _line_num = 0;
    private bool _isShow = false;

    private int update_count = 0, old_update_count = 0;

    public delegate void TraceLogDelegate(string log);
    public event TraceLogDelegate TraceLogPrinted;

    private void Awake()
    {
        if (!_container.activeSelf)
        {
            _isShow = false;
            _container.transform.localScale = Vector3.zero;
            _container.SetActive(true);
        }
        else
        {
            _isShow = true;
        }

        _print("> " + GetType().Name + " / Start");
        _versionText.text = "Version. " + Application.version;
        if (showDebugLog) Application.logMessageReceivedThreaded += OnLogMessage;
    }

    public static void Log(params object[] strs)
    {
        if (TraceBox.instance == null) return;
        if (strs[0].Equals(lastDebugMessage)) { lastDebugMessage = ""; return; }
        TraceBox.instance._print(strs);
        lastTraceMessage = strs[0].ToString();
    }

    static string lastDebugMessage = "";
    private void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (lastTraceMessage.Equals(condition)) { lastTraceMessage = ""; return; }
        string message = "";
        switch (type)
        {
            case LogType.Warning:
                message = $"<color=#ffDF72>[Warning] {condition} {stackTrace}</color>";
                break;
            case LogType.Error:
                message = $"<color=#ff7272>[Error] {condition} {stackTrace}</color>";
                break;
            case LogType.Exception:
                message = $"<color=#ff7272>[Exception] {condition} {stackTrace}</color>";
                break;
            case LogType.Assert:
                message = $"<color=#ff7272>[Assert] {condition} {stackTrace}</color>";
                break;
            default:
                message = "[D] " + condition;
                break;
        }
        lastDebugMessage = condition;
        _print(message);
    }

    private Queue<int> textLengthQueue = new Queue<int>();
    int _xSbLength;

    static string lastTraceMessage = "";
    private void _print(params object[] strs)
    {
        if (_sb == null) _sb = new StringBuilder();

        _sb.AppendFormat("{0} > ", _line_num);
        for (int i = 0; i < strs.Length; i++)
        {
            _sb.Append(strs[i].ToString());
            TraceLogPrinted?.Invoke(strs[i].ToString());
            _sb.Append(",");
        }
        _sb.AppendLine();

        textLengthQueue.Enqueue(_sb.Length - _xSbLength);
        _line_num++;

        int cur_line_count = _sb.ToString().Split('\n').Length;
        int over_line_num = cur_line_count - limitLineNum - 1;

        if (over_line_num > 0)
        {
            for (int i = 0; i < over_line_num; i++)
            {
                _sb.Remove(0, textLengthQueue.Dequeue());
            }
        }

        _xSbLength = _sb.Length;
        update_count++;
    }

    bool isTextUpdated;

    void Update()
    {
        if (isTextUpdated) { scrollRect.verticalNormalizedPosition = 0; isTextUpdated = false; }

        if (old_update_count != update_count)
        {
            try
            {
                _text.text = _sb.ToString();
                old_update_count = update_count;
                if (autoUpdateScroll) isTextUpdated = true;
            }
            catch
            {
            }
        }
    }
}