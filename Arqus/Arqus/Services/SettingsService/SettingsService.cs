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

namespace Arqus
{
    class SettingsService
    {
        private static QTMNetworkConnection connection;
        public static List<SettingsGeneralCameraSystem> generalSettings;
              
        // We need to convert the CameraMode enum to a string that matches the API's
        static Dictionary<CameraMode, string> CameraModeString = new Dictionary<CameraMode, string>()
        {
            { CameraMode.ModeMarker, "Marker" },
            { CameraMode.ModeMarkerIntensity, "Marker Intensity" },
            { CameraMode.ModeVideo, "Video" }
        };
        
        public static async Task<bool> SetCameraMode(int id, CameraMode mode)
        {
            try
            {
                return await Task.Run(() => connection.SetCameraMode(id, CameraModeString[mode]));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static bool EnableImageMode(int id, bool enabled)
        {
            return connection.Protocol.SendXML(Packet.CameraImage(id, enabled));
        }
        
        public static void Initialize(QTMNetworkConnection qtmConnection)
        {
            connection = qtmConnection;

            connection.Protocol.GetGeneralSettings();
            generalSettings = connection.Protocol.GeneralSettings.cameraSettings;
        }

        public static List<SettingsGeneralCameraSystem> GetCameraSettings()
        {            
            return generalSettings;
        }           

        public static int GetCameraCount()
        {
            return generalSettings.Count;
        }

        public static List<ImageCamera> GetImageCameraSettings()
        {
            connection.Protocol.GetImageSettings();
            return connection.Protocol.ImageSettings.cameraList;
        }
        private static readonly string port = "7979";
        private static string baseUrl = "http://{0}:{1}/api/experimental/{2}";
        static HttpClient client = new HttpClient();
        
        public static void Dispose()
        {
            connection.Dispose();
        }

        // Used for capping the rate with which QTM will be notified
        // of camera settings changes
        static DateTime timeStamp = DateTime.UtcNow;

        public static async Task<bool> SetCameraSettings(int id, string settingsParameter, int value)
        {
            // Wait arbitrary 50 ms to update value
            if ((DateTime.UtcNow - timeStamp).TotalMilliseconds < 50)
                return false;
            
            // Create XML command
            string commandPacket = Packet.SettingsParameter(id, settingsParameter, value);

            // Update time stamp
            timeStamp = DateTime.UtcNow;

            try
            {
                return await Task.Run(() => connection.Protocol.SendXML(commandPacket));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}
