namespace IMFINE.Net.TCPManager
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    [Serializable]
    public class BaseClient
    {
        public TcpClient client;
        public string id = "";
        public string ip = "";
        public int port = 0;
        public bool isOnline = false;
        public long lack = 0;
        public CancellationTokenSource cancelTokenSource;

        public BaseClient(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public string Serialize()
        {
            return $"{id},{ip},{port}";
        }

        public void Deserialize(string value)
        {
            string[] values = value.Split(',');
            id = values[0];
            ip = values[1];
            int.TryParse(values[2], out port);
        }

        public void Clear()
        {
            client = null;
            id = string.Empty;
            ip = string.Empty;
            port = 0;
        }
    }

}

