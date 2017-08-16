using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Arqus.DataModels;
using Arqus.Services;

namespace Arqus
{
    class CameraProfiler : IDisposable
    {
        List<CameraProfile> cameraProfiles;
        Dictionary<int, Camera> cameras;

        /// <summary>
        /// Initializes profiler by setting the camera references and loading the profiles json file
        /// </summary>
        /// <param name="cameras">Camera dictionary containing all cameras in system</param>
        /// <param name="profilesFilename">Json file containing pre-defined camera profiles</param>
        public CameraProfiler(Dictionary<int, Camera> _cameras, string profilesFilename)
        {
            cameras = _cameras;
            LoadProfiles(profilesFilename);
        }

        private void LoadProfiles(string filename)
        {
            // Get assembly object
            Assembly assembly = typeof(SettingsService).Assembly;

            // Get General Settings file stream
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + filename))
            {
                // Create a stream reader to read from stream (duh)
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    // Read file to string
                    string jsonString = streamReader.ReadToEnd();

                    try
                    {   
                        // Parse json string into structure 
                        cameraProfiles = JsonConvert.DeserializeObject<List<CameraProfile>>(jsonString);
                    }
                    catch (Exception e)
                    {
                        Debug.Print("Something went wrong when parsing " + filename);
                        Debug.Print(e.Message);
                    }
                }                
            }            
        }

        public void Run()
        {
            foreach(KeyValuePair<int, Camera> camera in cameras)
            {
                foreach (CameraProfile cameraProfile in cameraProfiles)
                {
                    if (camera.Value.Model.ToLower() == cameraProfile.Model.ToLower())
                    {
                        camera.Value.Profile = cameraProfile;
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {           
            if(cameraProfiles != null)
            {
                cameraProfiles.Clear();
                cameraProfiles = null;
            }
        }
    }
}
