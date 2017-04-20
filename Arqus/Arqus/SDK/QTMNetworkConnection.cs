﻿using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Helpers;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;
using System.Net;
using Prism.Navigation;
using Xamarin.Forms;
using System.Threading.Tasks;
using QTMRealTimeSDK.Data;

namespace Arqus
{
    public class QTMNetworkConnection
    {
        public static string Version { get; private set; }

        private static string _ipAddress;
        private static string _password;
        private bool hasControl;
        
        private RTProtocol protocol;
        public RTProtocol Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }
        
        
        public QTMNetworkConnection(string ipAddress)
        {
            Connect(ipAddress, _password);
        }

        /// <summary>
        /// NOTE: This should only be used when an intial connection has already been made
        /// This might not be a good approach?
        /// </summary>
        public QTMNetworkConnection()
        {
            Protocol = new RTProtocol();

            // If an ipAddress is already set make a connection attempt
            // since this is assumed to be the default use case
            if (_ipAddress != null)
            {
                Connect(_ipAddress, _password);
            }
        }

        /// <summary>
        /// Set the IpAddress property of the object instance and makes a connection attempt
        /// </summary>
        /// <returns>boolean indicating success or failure</returns>
        public bool Connect(string ipAddress, string password = "")
        {
            if(IsValidIPv4(ipAddress))
            {
                _ipAddress = ipAddress;
                _password = password;
                bool response = Connect();
                return response;
            }

            return false;
        }


        /// <summary>
        /// Attempts to connect to a QTM host
        /// </summary>
        /// <returns>boolean indicating success or failure</returns>
        public bool Connect()
        {
            if (!Protocol.Connect(_ipAddress))
            {
                return false;
            }

            // Make a check to determine wether the password is correct
            // and that the object instance can take control and thereby
            // send commands and/or update settings
            if (!CheckControl())
            {
                return false;
            }

            string ver;
            Protocol.GetQTMVersion(out ver);
            Version = ver;

            return true;
        }

        /// <summary>
        /// Checks to determine if object instance can take control
        /// </summary>
        /// <returns>true if the instance is able to take control</returns>
        private bool CheckControl()
        {
            if (!TakeControl())
                return false;

            return ReleaseControl();
        }

        /// <summary>
        /// Takes control of the QTM host
        /// 
        /// NOTE: Only one client can have control at the time
        /// </summary>
        /// <returns>true if the client took control</returns>
        private bool TakeControl()
        {
            bool success = Protocol.TakeControl(_password);

            if(!success)
            {
                string response = Protocol.GetErrorString();
                Debug.WriteLine("Error: " + response);
            }

            return success;
        }

        /// <summary>
        /// Releases the control for the current client
        /// </summary>
        /// <returns>true if the client released control</returns>
        private bool ReleaseControl()
        {
            bool success = Protocol.ReleaseControl();

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

        public List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            try
            {
                if (Protocol.DiscoverRTServers(port))
                {
                    Debug.WriteLine(string.Format("QTM Servers: {0}", protocol.DiscoveryResponses.Count));
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
            protocol.ReceiveRTPacket(out packet);
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

        public bool SetCameraMode(int id, string mode)
        {
            if(!hasControl)
                hasControl = TakeControl();

            string packetString = Packet.Camera(id, mode);
            bool success = Protocol.SendXML(packetString);

            if (success && IsImage(mode))
            {
                SetImageStream(id, true);
            }
            else if (success)
            {
                SetImageStream(id, false);
            }
            else
            {
                Debug.WriteLine("Unable to Enable/Disbale image mode");
            }

            hasControl = !ReleaseControl();

            return success;
        }

        public bool SetImageStream(int id, bool enabled)
        {
            if(!hasControl)
                hasControl = TakeControl();

            // TODO: This should not be hardcoded, get the image setting and pass the resolution as a parameter instead
            string packetString = Packet.CameraImage(id, enabled, "1823", "1087");
            bool response = Protocol.SendXML(packetString);
            hasControl = !ReleaseControl();

            return response;
        }
       

        private bool IsImage(string mode)
        {
            return mode != "Marker";
        }


    }
}
