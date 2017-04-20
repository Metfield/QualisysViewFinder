using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using QTMRealTimeSDK;
using Arqus.Helpers;

namespace Arqus
{
    class SettingsService : ISettingsService
    {
        private QTMNetworkConnection qtmConnection;
        
        // We need to convert the CameraMode enum to a string that matches the API's
        Dictionary<CameraMode, string> CameraModeString = new Dictionary<CameraMode, string>()
        {
            { CameraMode.ModeMarker, "Marker" },
            { CameraMode.ModeMarkerIntensity, "Marker Intensity" },
            { CameraMode.ModeVideo, "Video" }
        };

        public SettingsService()
        {
            qtmConnection = new QTMNetworkConnection(); 
        }        

        public async Task<bool> SetCameraMode(int id, CameraMode mode)
        {
            try
            {
                return await Task.Run(() => qtmConnection.SetCameraMode(id, CameraModeString[mode]));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
        
        public async Task<bool> SetCameraSettings(int id, string settingsParameter, int value)
        {
            string commandPacket = Packet.SettingsParameter(id, settingsParameter, value);
            string result;

            bool blah = qtmConnection.Protocol.SendXML(commandPacket);

            try
            {
                return await Task.Run(() => qtmConnection.Protocol.SendCommandExpectXMLResponse(commandPacket, out result));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}
