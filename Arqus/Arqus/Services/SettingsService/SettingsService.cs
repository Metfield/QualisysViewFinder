using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using QTMRealTimeSDK;
using Arqus.Helpers;
using QTMRealTimeSDK.Settings;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;

namespace Arqus
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
                return connection.SetCameraMode(id, CameraModeString[mode]);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static bool EnableImageMode(int id, bool enabled, int width, int height)
        {
            return connection.SetImageStream(id, enabled, width, height);
        }

        public static bool SetImageResolution(int id, int width, int height)
        {
            return connection.SetImageResolution(id, width, height);
        }

        /// <summary>
        /// Initializes settings service.
        /// </summary>
        /// <param name="demoMode">Initialize in demo mode (not real time)</param>
        public static void Initialize(bool demoMode = false)
        {
            isDemoModeActive = demoMode;

            if (!demoMode) // Real-time
            {
                //connection = new QTMNetworkConnection();
                connection.Connect();

                // Get the first one manually and then let the auto-update run
                connection.Protocol.GetGeneralSettings();
                generalSettings = connection.Protocol.GeneralSettings.CameraSettings;
            }    
            else // Demo mode
            {
                // Load both general and image settings
                if(generalSettings == null)
                    generalSettings = LoadGeneralSettings();

                if(imageCameras == null)
                    imageCameras = LoadImageSettings();               
            }
        }

        private static List<SettingsGeneralCameraSystem> LoadGeneralSettings()
        {
            // Get assembly object
            Assembly assembly = typeof(SettingsService).Assembly;

            // Get General Settings file
            using (Stream stream = assembly.GetManifestResourceStream("Arqus.RunningGeneralSettings.xml"))
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
            using (Stream stream = assembly.GetManifestResourceStream("Arqus.RunningImageSettings.xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsImage));
                SettingsImage si = (SettingsImage)xmlSerializer.Deserialize(stream);

                // Store mock settings
                return si.Cameras;
            }
        }

        /// <summary>
        /// Fetch camera settings every dt miliseconds 
        /// 
        /// NOTE!!!! This is currently NOT used as getting
        /// General Settings is a problem right now.
        /// 
        /// TODO: Try when things are more stable
        /// </summary>
        private static async void RefreshSettings()
        {
            while (connection.Protocol.IsConnected())
            {
                // Can this even fail?
                while (!connection.Protocol.GetGeneralSettings())
                {
                    Task.Delay(100);
                }

                generalSettings = connection.Protocol.GeneralSettings.CameraSettings;
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

            // Refresh general settings
            connection.Protocol.GetGeneralSettings();

            try
            {
                // Try and fetch the new settings
                generalSettings = connection.Protocol.GeneralSettings.CameraSettings;
            }
            catch (Exception e)
            {
                Debug.Print("SettingsService::GetCameraSettings Exception!.. " + e.Message);
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
        public static bool SetCameraSettings(int id, string settingsParameter, string value)
        {     
            try
            {
                return connection.SetCameraSettings(id, settingsParameter, value);
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
            try
            {                
                bool response = await Task.Run(() => connection.SetLED(id, mode.ToString(), color.ToString()));
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
            
        }            

        public static void Clean()
        {
            if(connection != null)
                connection.Dispose();

            isDemoModeActive = false;

            generalSettings = null;
            imageCameras = null;
        }
    }
}
