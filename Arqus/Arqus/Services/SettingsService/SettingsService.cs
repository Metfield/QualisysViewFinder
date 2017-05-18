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

namespace Arqus
{
    class SettingsService
    {
        private static QTMNetworkConnection connection = new QTMNetworkConnection();
        public static List<SettingsGeneralCameraSystem> generalSettings;

        // We need to convert the CameraMode enum to a string that matches the API's
        static Dictionary<CameraMode, string> CameraModeString = new Dictionary<CameraMode, string>()
        {
            { CameraMode.ModeMarker, "Marker" },
            { CameraMode.ModeMarkerIntensity, "Marker Intensity" },
            { CameraMode.ModeVideo, "Video" }
        };

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

        public static void Initialize()
        {
            connection.Connect();
            // Get the first one manually and then let the auto-update run
            connection.Protocol.GetGeneralSettings();
            generalSettings = connection.Protocol.GeneralSettings.CameraSettings;

            // Start periodic fetching of camera settings in a 
            // separate thread
            //Task.Run(() => RefreshSettings());
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

        public static SettingsGeneralCameraSystem GetCameraSettings(int id)
        {
            try
            {
                connection.Protocol.GetGeneralSettings();
            }
            catch (Exception e)
            {
                Debug.Print("SettingsService::GetCameraSettings Exception!.. " + e.Message);
            }

            return connection.Protocol.GeneralSettings.CameraSettings.Where(camera => camera.CameraId == id).First();
        }

        public static int GetCameraCount()
        {
            return generalSettings.Count;
        }

        public static List<ImageCamera> GetImageCameraSettings()
        {
            connection.Protocol.GetImageSettings();
            return connection.Protocol.ImageSettings.Cameras;
        }


        // Used for capping the rate with which QTM will be notified
        // of camera settings changes
        static DateTime timeStamp = DateTime.UtcNow;

        /// <summary>
        /// Sends new settings to QTM
        /// </summary>
        /// <param name="id">Camera ID</param>
        /// <param name="settingsParameter">Parameter to send</param>
        /// <param name="value">Parameter's value</param>
        /// <returns>Returns true if successful</returns>
        public static bool SetCameraSettings(int id, string settingsParameter, float value)
        {
            // Wait arbitrary 50 ms to update value
            if ((DateTime.UtcNow - timeStamp).TotalMilliseconds < 50)
                return false;

            // Update time stamp
            timeStamp = DateTime.UtcNow;

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

        public static void Dispose()
        {
            connection.Dispose();
        }

    }
}
