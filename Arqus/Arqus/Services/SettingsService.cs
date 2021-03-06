﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using QTMRealTimeSDK.Settings;
using System.Linq;
using System.Reflection;
using Arqus.Helpers;
using System.Xml.Serialization;

namespace Arqus.Services
{
    class SettingsService
    {
        private static QTMNetworkConnection connection = new QTMNetworkConnection();

        public static List<SettingsGeneralCameraSystem> generalSettings = null;
        private static List<ImageCamera> imageCameras = null;

        // We need to convert the CameraMode enum to a string that matches the API's
        static Dictionary<CameraMode, string> CameraModeString = new Dictionary<CameraMode, string>()
        {
            { CameraMode.ModeMarker, "Marker" },
            { CameraMode.ModeMarkerIntensity, "Marker Intensity" },
            { CameraMode.ModeVideo, "Video" }
        };

        private static bool isDemoModeActive;

        public static bool SetCameraMode(int id, CameraMode mode)
        {
            try
            {
                return MasterDelegate(() => connection.SetCameraMode(id, CameraModeString[mode]));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static bool MasterDelegate(Func<bool> fun)
        {
            if (QTMNetworkConnection.IsMaster)
                return fun();
            return true;
        }

        public static bool DisableImageMode(int id)
        {
            return MasterDelegate(() => connection.SetImageStream(id, false));
        }


        public static bool EnableImageMode(int id, bool enabled, int width, int height)
        {

            return MasterDelegate(() => connection.SetImageStream(id, enabled, width, height));
        }

        public static bool SetImageResolution(int id, int width, int height)
        {
            return MasterDelegate(() => connection.SetImageResolution(id, width, height));
        }

        /// <summary>
        /// Initializes settings service.
        /// </summary>
        /// <param name="demoMode">Initialize in demo mode (not real time)</param>
        public static bool Initialize(bool demoMode = false)
        {
            isDemoModeActive = demoMode;

            if (!demoMode) // Real-time
            {
                if (!connection.Connect(connection.GetRandomPort()))
                    return false;

                // Get the first one manually and then let the auto-update run
                if (connection.Protocol.GetGeneralSettings())
                    generalSettings = connection.Protocol.GeneralSettings.CameraSettings;
                else
                    return false;
            }    
            else // Demo mode
            {
                // Load both general and image settings
                if(generalSettings == null)
                    generalSettings = LoadGeneralSettings();

                if(imageCameras == null)
                    imageCameras = LoadImageSettings();               
            }

            return true;
        }

        private static List<SettingsGeneralCameraSystem> LoadGeneralSettings()
        {
            // Get assembly object
            Assembly assembly = typeof(SettingsService).Assembly;

            // Get General Settings file
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".RunningGeneralSettings.xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsGeneral));
                SettingsGeneral gs = (SettingsGeneral)xmlSerializer.Deserialize(stream);

                // Store mock settings
                return gs.CameraSettings;
            }
        }

        private static List<ImageCamera> LoadImageSettings()
        {
            // Get assembly object
            Assembly assembly = typeof(SettingsService).Assembly;

            // Get General Settings file
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".RunningImageSettings.xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsImage));
                SettingsImage si = (SettingsImage)xmlSerializer.Deserialize(stream);

                // Store mock settings
                return si.Cameras;
            }
        }

        /// <summary>
        /// Get fresh General Settings
        /// </summary>
        /// <returns></returns>
        public static List<SettingsGeneralCameraSystem> GetCameraSettings()
        {
            if (isDemoModeActive)
                return generalSettings;

            // Refresh general settings, return previous one if there was an error when fetching it
            if (!connection.Protocol.GetGeneralSettings())
                return generalSettings;

            if (connection.Protocol.GetErrorString() == "Camera system not running")
            {
                QTMNetworkConnection.ConnectionIsRecordedMeasurement = true;
                return generalSettings;
            }

            try
            {
                // Try and fetch the new settings
                generalSettings = connection.Protocol.GeneralSettings.CameraSettings;
            }
            catch (Exception e)
            {
                Debug.Print("SettingsService::GetCameraSettings Exception!.. " + e.Message);
                Debugger.Break();
            }

            // If the 'try' fails, this will at least return the 
            // last fetched general settings
            return generalSettings;
        }

        /// <summary>
        /// Tries to retrieve camera settings
        /// </summary>
        /// <param name="id">id of the camera to get settings from</param>
        /// <returns>camera settings or null in case of failure</returns>
        public static SettingsGeneralCameraSystem? GetCameraSettings(int id)
        {
            try
            {
                if(connection.Protocol.GetGeneralSettings())
                    return connection.Protocol.GeneralSettings.CameraSettings.Where(camera => camera.CameraId == id)?.First();
            }
            catch (Exception e)
            {
                Debug.Print("Error: " + e.Message);
            }

            return null;
        }

        public static int GetCameraCount()
        {
            return generalSettings.Count;
        }

        public static List<ImageCamera> GetImageCameraSettings()
        {
            if (isDemoModeActive)
                return imageCameras;

            connection.Protocol.GetImageSettings();
            return connection.Protocol.ImageSettings.Cameras;
        }
        
        /// <summary>
        /// Sends new settings to QTM
        /// </summary>
        /// <param name="id">Camera ID</param>
        /// <param name="settingsParameter">Parameter to send</param>
        /// <param name="value">Parameter's value</param>
        /// <returns>Returns true if successful</returns>
        public static bool SetCameraSettings(Packet.Type packetType, int id, string settingsParameter, string value)
        {
            if (isDemoModeActive)
                return false;

            try
            {
                return MasterDelegate(() => connection.SetCameraSettings(packetType, id, settingsParameter, value));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public enum LEDMode
        {
            On,
            Off,
            Pulsing
        }

        public enum LEDColor
        {
            Green,
            Amber,
            All
        }

        public static async Task<bool> SetLED(int id, LEDMode mode, LEDColor color = LEDColor.All)
        {
            if (isDemoModeActive)
                return false;

            try
            {
                bool response = await Task.Run(() => MasterDelegate(() => connection.SetLED(id, mode.ToString(), color.ToString())));
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        // Starts new measurement, provided that a connection has been made
        public static bool StartMeasurement()
        {
            return connection.Protocol.NewMeasurement();
        }

        public static void Clean()
        {
            QTMNetworkConnection.ConnectionIsRecordedMeasurement = false;

            if (connection != null)
                connection.Dispose();

            isDemoModeActive = false;

            if (generalSettings != null)
            {
                generalSettings.Clear();
                generalSettings = null;
            }

            if (imageCameras != null)
            {
                imageCameras.Clear();
                imageCameras = null;
            }
        }

        public static bool CropImage(int id, float left, float right, float top, float bottom)
        {
            return MasterDelegate(() => connection.CropImage(id, left, right, top, bottom));
        }
    }
}
