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
        private static QTMNetworkConnection connection = new QTMNetworkConnection();
        
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
        
        public static List<SettingsGeneralCameraSystem> GetCameraSettings()
        {
            connection.Protocol.GetGeneralSettings();
            return connection.Protocol.GeneralSettings.cameraSettings;
        }

        public static List<ImageCamera> GetImageCameraSettings()
        {
            connection.Protocol.GetImageSettings();
            return connection.Protocol.ImageSettings.cameraList;
        }
        private static readonly string port = "7979";
        private static string baseUrl = "http://{0}:{1}/api/experimental/{2}";
        static HttpClient client = new HttpClient();

        /*private static string GetUrl(string resource)
        {
            //return string.Format(baseUrl, QTMNetworkConnection.IpAddress, port, resource);
        }
        */

        /*
        public static async Task<List<object>> GetCameraSettings()
        {
            try
            {
                var uri = new Uri(GetUrl("settings"));
                var response = await client.GetAsync(uri);

                if(response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Settings settings = JsonConvert.DeserializeObject<Settings>(content);
                    return settings.Cameras;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Failed to get camera settings. \n Error: " + e);
            }
            return null;
        }
        */
        
        public static void Dispose()
        {
            connection.Dispose();
        }
        public static async Task<bool> SetCameraSettings(int id, string settingsParameter, int value)
        {
            string commandPacket = Packet.SettingsParameter(id, settingsParameter, value);
            string result;

            bool blah = connection.Protocol.SendXML(commandPacket);

            try
            {
                return await Task.Run(() => connection.Protocol.SendCommandExpectXMLResponse(commandPacket, out result));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}
