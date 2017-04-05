using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QTMRealTimeSDK;

namespace Arqus.Services
{
    class RESTfulQTMService : IRESTfulQTMService
    {
        HttpClient client;
        private string port = "7979";
        private string baseUrl = "http://{0}:{1}/api/experimental/{2}";
        public string Ip { get; set; }

        // We need to convert the CameraMode enum to a string that matches the API's
        Dictionary<CameraMode, string> CameraModeString = new Dictionary<CameraMode, string>()
        {
            { CameraMode.ModeMarker, "Marker" },
            { CameraMode.ModeMarkerIntensity, "Intensity" },
            { CameraMode.ModeVideo, "Video" }
        };

        public RESTfulQTMService(string ip)
        {
            client = new HttpClient();
            Ip = ip;
            
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.MaxResponseContentBufferSize = 256000;
        }

        
        public async void GetSettings()
        {
            try
            {
                var uri = new Uri(string.Format(baseUrl, Ip, port, "settings"));
                Debug.WriteLine(uri);

                var response = await client.GetAsync(uri);

                if(response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Settings set = JsonConvert.DeserializeObject<Settings>(content);
                    Debug.WriteLine(set.ToString());
                }
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async void SetCameraMode(uint id, CameraMode mode)
        {
            try
            {
                var uri = new Uri(string.Format(baseUrl, Ip, port, "settings"));
                Debug.WriteLine(uri);
                

                string rawRequest = "{\"Cameras\":[{\"Id\":" + id + ",\"Mode\":\"" + CameraModeString[mode] + "\"}]}";
                var request = rawRequest;

                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(res.ToString());
                    CameraStream.Instance.SwitchStreamingMode(id, mode);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
