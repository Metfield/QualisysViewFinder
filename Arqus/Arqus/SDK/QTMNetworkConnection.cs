﻿using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Helpers;
using System.Diagnostics;
using QTMRealTimeSDK.Settings;
using System.Net;
using QTMRealTimeSDK.Data;
using System.Text.RegularExpressions;

namespace Arqus
{
    public class QTMNetworkConnection : IDisposable
    {
        public static string Version { get; private set; }

        public static bool IsMaster { get; set; }

        private static string ipAddress;
        private static string password;

        private bool hasControl = false;
        private static readonly object controlLock = new object();
        
        public static RTProtocol Master { get; set; }
        public RTProtocol Protocol { get; private set; }

        public static bool QTMVersionSupported { get; set; }

        public static bool ConnectionIsRecordedMeasurement { get; set; }

        public const short MINIMUM_SUPPORTED_QTM_VERSION = 216;
        
        public QTMNetworkConnection()
        {
            Protocol = new RTProtocol();
            Connect(GetRandomPort());

            QTMVersionSupported = true;
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
            
            return Connect(GetRandomPort());
        }
        
        /// <summary>
        /// Attempts to connect to a QTM host
        /// </summary>
        /// <returns>boolean indicating success or failure</returns>
        public bool Connect(int port)
        {
            if(IsValidIPv4(ipAddress))
            {
                hasControl = false;
                if (Protocol.Connect(ipAddress, port))
                {
                    string ver;
                    Protocol.GetQTMVersion(out ver);
                    Version = ver;

                    if (!QTMVersionIsSupported())
                    {
                        QTMVersionSupported = false;
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        // Check if the QTM version to which we've connected is supported
        // IMPORTANT - Assuming that RTProtocol returns QTM version in the 
        // form of "QTM version is M.mm build bbbb"
        private bool QTMVersionIsSupported()
        {
            string versionString = Regex.Replace(Version, "[^0-9 _]", "");

            while(versionString.ElementAt(0) == ' ')
                versionString = versionString.Remove(0, 1);
            
            versionString = versionString.Remove(versionString.IndexOf(" ") + 1);

            int QTMMajorVersion= int.Parse(versionString[0].ToString());
            int compatibleMajorVersion = int.Parse(MINIMUM_SUPPORTED_QTM_VERSION.ToString()[0].ToString());

            // Check major version
            if (QTMMajorVersion > compatibleMajorVersion)
                return true;

            if (QTMMajorVersion < compatibleMajorVersion)
                return false;

            // It's version 2, but is minor bigger or equal than 16?
            if (int.Parse(versionString) < MINIMUM_SUPPORTED_QTM_VERSION)
                return false;

            return true;
        }

        /// <summary>
        /// Takes control of the QTM host
        /// 
        /// NOTE: Only one client can have control at the time
        /// </summary>
        /// <returns>true if the client took control</returns>
        public bool TakeControl()
        {
            if (hasControl)
                return true;

            if (Master != null)
                Master.ReleaseControl();
            
            if (!Protocol.TakeControl(password))
            {
                string response = Protocol.GetErrorString();

                if(response.Contains("is already master"))
                {
                    Master = Protocol;
                    return true;
                }

                IsMaster = false;

                Debug.WriteLine("Error: " + response);
                return false;
            }

            Master = Protocol;
            IsMaster = true;

            hasControl = true;

            return true;
        }

        public bool HasPassword()
        {
            return !TakeControl();
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

            hasControl = false;

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

        public List<DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
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

            string response;
            bool success;

            lock (controlLock)
            {
                TakeControl();
                success = Protocol.SendXML(packetString, out response);
            }

            return true;
        }

        public bool SetImageStream(int id, bool enabled)
        {
            return SetImageStream(Packet.CameraImage(id, enabled));
        }

        public bool SetImageStream(int id, bool enabled, int width, int height)
        {            
            return SetImageStream(Packet.CameraImage(id, enabled, width, height));
        }

        private bool SetImageStream(string packet)
        {
            string response;

            bool success;

            lock (controlLock)
            {
                TakeControl();
                success = Protocol.SendXML(packet, out response);
            }

            return success;
        }

        public bool SetImageResolution(int id, int width, int height)
        {
            try
            {

                string response;
                string packetString = Packet.CameraImage(id, width, height);

                lock (controlLock)
                {
                    TakeControl();
                    return Protocol.SendXML(packetString, out response);
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

        public bool CropImage(int id, float left, float right, float top, float bottom)
        {
            try
            {
                string response;
                string packet = Packet.CropImage(id, left, right, top, bottom);
                bool success = Protocol.SendXML(packet, out response);
                HandleHostResponse(response);
                return success;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public bool SetCameraSettings(Packet.Type packetType, int id, string settingsParameter, string value)
        {
            string response;
            string packetString;

            // Create XML command depending on type of camera settings parameter
            switch(packetType)
            {
                case Packet.Type.LensControl:
                    packetString = Packet.LensControlParameter(id, settingsParameter, value);
                    break;

                case Packet.Type.AutoExposure:
                    packetString = Packet.AutoExposureParameter(id, settingsParameter, value);
                    break;

                case Packet.Type.Default:
                default:
                    packetString = Packet.SettingsParameter(id, settingsParameter, value);
                    break;
            }

            // Take control and issue command
            TakeControl();

            bool success = Protocol.SendXML(packetString, out response);
            HandleHostResponse(response);

            return success;
        }

        private bool SettingIsLensControl(string setting)
        {
            if (setting == Constants.LENS_APERTURE_PACKET_STRING || setting == Constants.LENS_FOCUS_PACKET_STRING)
                return true;

            return false;
        }

        // Handle response appropriately
        // TODO: Handle other messages        
        private void HandleHostResponse(string response)
        {
            // Right now just print in debug mode
            Debug.Print("QTM Response: " + response);
        }

        private bool IsImage(string mode)
        {
            return mode != "Marker";
        }

        // Used for UDP connection, gets random port from a pre-designated range
        public int GetRandomPort()
        {
            Random rand = new Random();
            return rand.Next(1024, 65534);
        }

        // Checks if there is any active measurement in the system
        // NOTE: Streaming needs to be enabled in order for this to work properly
        public bool IsMeasurementActive()
        {
            PacketType packetType;
            Protocol.ReceiveRTPacket(out packetType);

            // If this is false, there are active measurements
            if (packetType == PacketType.PacketNoMoreData)
                return false;
            else
                return true;
        }

        public void Dispose()
        {
            Protocol.ReleaseControl();
            Protocol.Dispose();
        }
    }
}

