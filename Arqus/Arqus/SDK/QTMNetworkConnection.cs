using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Helpers;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;
using System.Net;
using QTMRealTimeSDK.Data;

namespace Arqus
{
    public class QTMNetworkConnection : IDisposable
    {
        public static string Version { get; private set; }

        private static string ipAddress;
        private static string password;
        private static readonly object padlock = new object();


        public static RTProtocol Master { get; set; }
        public RTProtocol Protocol { get; private set; }
        
        public QTMNetworkConnection()
        {
            Protocol = new RTProtocol();
            Connect();
        }


        public QTMNetworkConnection(int port)
        {
            Protocol = new RTProtocol();
            Connect(port);
        }

        /// <summary>
        /// Attempts to connect to a QTM host
        /// </summary>
        /// <returns>boolean indicating success or failure</returns>
        public bool Connect(string ipAddress, string password = "")
        {
            lock(padlock)
            {
                QTMNetworkConnection.ipAddress = ipAddress;
                QTMNetworkConnection.password = password;
            }

            return Connect();
        }
        


        /// <summary>
        /// Attempts to connect to a QTM host
        /// </summary>
        /// <returns>boolean indicating success or failure</returns>
        public bool Connect(int port = -1)
        {
            if(IsValidIPv4(ipAddress))
            {
                if (Protocol.Connect(ipAddress, port))
                {
                    string ver;
                    Protocol.GetQTMVersion(out ver);
                    Version = ver;
                    return TakeControl();
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Takes control of the QTM host
        /// 
        /// NOTE: Only one client can have control at the time
        /// </summary>
        /// <returns>true if the client took control</returns>
        private bool TakeControl()
        {
            if(!ReferenceEquals(Master, Protocol))
            {
                if (Master != null)
                    ReleaseControl();

                if (!Protocol.TakeControl(password))
                {
                    string response = Protocol.GetErrorString();
                    Debug.WriteLine("Error: " + response);
                    return false;
                }
                
                lock(padlock)
                {
                    Master = Protocol;
                }
            }

            return true;
        }

        /// <summary>
        /// Releases the control for the current client
        /// </summary>
        /// <returns>true if the client released control</returns>
        private bool ReleaseControl()
        {
            bool success = Master.ReleaseControl();

            if(!success)
            {
                string response = Protocol.GetErrorString();
                Debug.WriteLine("Error: " + response);
            }

            return success;
        }

        /// <summary>
        /// Makes sure ipAddress string is a valid IPv4
        /// </summary>
        /// <param name="ipString">Holds QTM instance IP address</param>
        /// <returns></returns>
        public static bool IsValidIPv4(string ipString)
        {
            // Check for null string
            if (ipString == null)
                return false;

            // Check if it's made of four elements
            if (ipString.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length != 4)
                return false;

            IPAddress address;

            // Check if this is a valid IP address
            if (System.Net.IPAddress.TryParse(ipString, out address))
            {
                // Make sure it's an ipv4 (although it should)
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            // TODO: Check if address is in LAN and in a valid range!

            return false;
        }

        static List<ComponentType> streamTypes = new List<ComponentType>();

        public bool UpdateStream(ComponentType type)
        {
            lock(padlock)
            {
                streamTypes.Add(type);
            }

            return Protocol.StreamFrames(StreamRate.RateAllFrames, 30, streamTypes);
        }

        public List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            try
            {
                if (Protocol.DiscoverRTServers(port))
                {
                    Debug.WriteLine(string.Format("QTM Servers: {0}", Protocol.DiscoveryResponses.Count));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return Protocol.DiscoveryResponses.ToList();
        }

        public List<ImageCamera> GetImageSettings()
        {
            PacketType packet;
            Protocol.ReceiveRTPacket(out packet);
            var imageSettings = Protocol.GetImageSettings();
            return imageSettings ? Protocol.ImageSettings.cameraList : null;
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

        public bool SetCameraMode(int id, string mode)
        {
            string packetString = Packet.Camera(id, mode);

            TakeControl();
            if(Protocol.SendXML(packetString))
            {
                SetImageStream(id, IsImage(mode));
            }
            else
            {
                Debug.WriteLine("Unable to Enable/Disable image mode");
                return false;
            }
            
            return true;
        }

        public bool SetImageStream(int id, bool enabled)
        {
            string packetString = Packet.CameraImage(id, enabled);
            TakeControl();
            return Protocol.SendXML(packetString);
        }
       

        private bool IsImage(string mode)
        {
            return mode != "Marker";
        }

        public void Dispose()
        {
            Protocol.Dispose();
        }
    }
}
