using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Helpers;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;

namespace Arqus
{
    public sealed class QTMNetworkConnection
    {
        private static readonly QTMNetworkConnection instance = new QTMNetworkConnection();
        
        public string IPAddress { set; get; }
        public RTProtocol protocol { private set; get; }

        string version;
        public string Version { get; private set; }

        static QTMNetworkConnection() { }

        private QTMNetworkConnection()
        {
            // Default value is localhost
            IPAddress = "127.0.0.1";
            protocol = new RTProtocol();
        }

        public static QTMNetworkConnection Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Connect to previously set IP
        /// </summary>
        /// <returns></returns>
        public bool Connect(string ipAddress)
        {
            IPAddress = ipAddress;
            return Connect();
        }

        public bool Connect()
        {
            if (!protocol.Connect(IPAddress))
            {
                return false;
            }

            protocol.GetQTMVersion(out version);
            return true;
        }
       
        public List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            if (protocol.DiscoverRTServers(port))
            {
                if (protocol.DiscoveryResponses.Count == 0)
                {
                    Debug.WriteLine("No QTM Servers were found");
                }

                return protocol.DiscoveryResponses.ToList();
            }

            return null;
        }

        public List<ImageCamera> GetImageSettings()
        {
            var imageSettings = protocol.GetImageSettings();
            return imageSettings ? protocol.ImageSettings.cameraList : null;
        }

        public IEnumerable<ImageResolution> GetAllCameraResolutions()
        {
            return GetImageSettings().Select(camera => new ImageResolution(camera.Width, camera.Height));
        }

        public ImageResolution GetCameraResolution(int cameraID)
        {
            ImageCamera camera = GetImageSettings().Where(c => c.CameraID == cameraID).First();
            return new ImageResolution(camera.Width, camera.Height);
        }
    }
}
