namespace IMFINE.Net.TCPManager
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TCPManager))]
    public class TCPManagerEditor : Editor
    {
        private SerializedProperty enableAutoConnection;
        private SerializedProperty isServer;
        private SerializedProperty id;
        private SerializedProperty serverIp;
        private SerializedProperty serverPort;
        private SerializedProperty enableDetailedLog;
        private SerializedProperty enableMessageLog;

        public void OnEnable()
        {
            enableAutoConnection = serializedObject.FindProperty("_enableAutoConnection");
            isServer = serializedObject.FindProperty("_isServer");
            id = serializedObject.FindProperty("_id");
            serverIp = serializedObject.FindProperty("_serverIp");
            serverPort = serializedObject.FindProperty("_serverPort");
            enableDetailedLog = serializedObject.FindProperty("_enableDetailedLog");
            enableMessageLog = serializedObject.FindProperty("_enableMessageLog");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(enableAutoConnection, new GUIContent("Enable Auto Connection"));

            EditorGUI.indentLevel = 1;
            EditorGUI.BeginDisabledGroup(!enableAutoConnection.boolValue);
            EditorGUILayout.PropertyField(isServer, new GUIContent("Is Server"));
            EditorGUILayout.PropertyField(id, new GUIContent("ID"));
            id.stringValue = id.stringValue.ToUpper();
            EditorGUILayout.HelpBox("Client 식별을 위해 사용합니다. 다른 Client와 동일한 ID를 사용할 수 없습니다.", MessageType.None);
            if (isServer.boolValue)
            {
                serverIp.stringValue = NetworkUtility.GetLocalIPAddress();
                EditorGUILayout.LabelField("Server IP", serverIp.stringValue);
            }
            else EditorGUILayout.PropertyField(serverIp, new GUIContent("Server IP"), false);
            EditorGUILayout.HelpBox("TCP 통신에 사용할 서버의 IP 주소.", MessageType.None);
            //if (isServer.boolValue) EditorGUI.EndDisabledGroup();
            serverPort.intValue = Mathf.Clamp(EditorGUILayout.IntField("Server Port", serverPort.intValue), 49152, 65535);
            EditorGUILayout.HelpBox("TCP 통신에 사용할 서버의 Port 번호. 49152 ~ 65535 범위의 값을 권장합니다.", MessageType.None);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.PropertyField(enableDetailedLog, new GUIContent("Enable Detailed Log"));
            EditorGUILayout.PropertyField(enableMessageLog, new GUIContent("Enable Message Log"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}