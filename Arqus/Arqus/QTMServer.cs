namespace Arqus.Connection
{
    class QTMServer
    {
        public string IPAddress { set; get; }
        public string HostName { set; get; }
        public string Port { set; get; }
        public string InfoText { set; get; }
        public string CameraCount { set; get; }

        public QTMServer(string ipAddress, string hostName, string port, string infoText, string cameraCount)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            Port = port;
            InfoText = infoText;
            CameraCount = cameraCount;
        }

        public string GetDetails()
        {
            return IPAddress + ":" + Port + ", " + InfoText + ", Camera count: " + CameraCount;
        }
    }

}
