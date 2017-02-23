using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Camera2D;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;

namespace Arqus
{
    public sealed class QTMNetworkConnection
    {
        private static readonly QTMNetworkConnection instance = new QTMNetworkConnection();
        
        public string IPAddress { set; get; }
        public RTProtocol protocol { private set; get; }


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
        public bool Connect()
        {
            // Check if we're already connected
            if(!protocol.IsConnected())
            {
                // Return false if connection was not successfull
                if(!protocol.Connect(IPAddress))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Connect to this new IP
        /// </summary>
        /// <param name="ipAddress">New IP to connect to</param>
        /// <returns></returns>
        public bool Connect(string ipAddress)
        {
            // Set IP and try to connect
            IPAddress = ipAddress;
            Debug.WriteLine(IPAddress);
            return Connect();
        }
       
        public List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            if (protocol.DiscoverRTServers(port))
            {
                Debug.WriteLine("Found RT servers");
                return protocol.DiscoveryResponses
                    .ToList();
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
