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

        private bool hasControl = false;
        private static readonly object controlLock = new object();
        
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
            lock(controlLock)
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
                    return true;
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
            if (hasControl)
                return true;

            if (Master != null)
                Master.ReleaseControl();
            
            if (!Protocol.TakeControl(password))
            {
                string response = Protocol.GetErrorString();
                Debug.WriteLine("Error: " + response);
                return false;
            }

            Master = Protocol;
            hasControl = true;

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
        /// End Connection
        /// </summary>
        public void Disconnect()
        {
            Protocol.Disconnect();
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

        PacketType packet;

        public List<ImageCamera> GetImageSettings()
        {
            Protocol.ReceiveRTPacket(out packet);
            var imageSettings = Protocol.GetImageSettings();
            return imageSettings ? Protocol.ImageSettings.Cameras: null;
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
            
            bool success;

            lock (controlLock)
            {
                TakeControl();
                success = Protocol.SendXML(packetString);
            }

            return true;
        }
        

        public bool SetImageStream(int id, bool enabled)
        {
            
            string packetString = Packet.CameraImage(id, enabled);
            

            bool success;

            lock (controlLock)
            {
                TakeControl();
                success = Protocol.SendXML(packetString);
            }

            return success;
        }

        public bool SetImageResolution(int id, int width, int height)
        {
            try
            {
                string packetString = Packet.CameraImage(id, width, height);
                lock (controlLock)
                {
                    TakeControl();
                    return Protocol.SendXML(packetString);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public bool SetLED(int id, string mode, string color)
        {
            try
            {
                string command = string.Format("Led {0} {1} {2}", id, mode, color);

                lock(controlLock)
                {
                    TakeControl();
                    Protocol.SendString(command, PacketType.PacketCommand);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
       
        public bool SetCameraSettings(int id, string settingsParameter, float value)
        {
            // Create XML command
            string packetString = Packet.SettingsParameter(id, settingsParameter, value);
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

