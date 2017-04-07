using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Helpers;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;

namespace Arqus
{
    public class QTMNetworkConnection
    {
        
        static string version;
        public static string Version { get; private set; }

        private static string ipAddress;

        public static string IpAddress
        {
            get { return ipAddress; }
            private set { ipAddress = value; }
        }


        private RTProtocol protocol;
        public RTProtocol Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }
        
        public QTMNetworkConnection()
        {
            Protocol = new RTProtocol();

            if(IpAddress != null)
            {
                Connect(ipAddress);
            }
        }

        public QTMNetworkConnection(string ipAddress)
        {
            Protocol = new RTProtocol();
            Connect(ipAddress);
        }

        /// <summary>
        /// Connect to previously set IP
        /// </summary>
        /// <returns></returns>
        public bool Connect(string ipAddress)
        {
            IpAddress = ipAddress;
            return Connect();
        }

        public bool Connect()
        {
            if (!protocol.Connect(IpAddress))
            {
                return false;
            }

            string ver;
            protocol.GetQTMVersion(out ver);

            protocol.TakeControl();
            if(protocol.GetErrorString() == "Wrong or missing password")
            {
                protocol.TakeControl("test");
            }

            Version = ver;
            return true;
        }
       
        public static List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            try
            {
                if (protocol.DiscoverRTServers(port))
                {
                    Debug.WriteLine(string.Format("QTM Servers: {0}", protocol.DiscoveryResponses.Count));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return protocol.DiscoveryResponses.ToList();
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
