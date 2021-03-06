﻿using QTMRealTimeSDK;
using Arqus.Visualization;

using System.Linq;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;
using Arqus.Helpers;
using Arqus.DataModels;
using Prism.Mvvm;
using System.Diagnostics;
using Arqus.Services;

namespace Arqus
{

    /// <summary>
    /// Camera store that handles retreival of up-to-date cameras and the current 
    /// state of the cameras in the application
    /// </summary>
    class CameraManager : BindableBase
    {
        private static Camera currentCamera;

        public static Camera CurrentCamera
        {
            get { return currentCamera; }
            set { currentCamera = value; }
        }

        public static Dictionary<int, Camera> Cameras;
        static SettingsService settingsService = new SettingsService();
        static public List<CameraScreen> Screens { get; set; }

        /// <summary>
        /// Generates camera screens to use inside an Urho context based on the 
        /// ImageCameras recieved from the QTM host.
        /// </summary>
        /// <returns></returns>
        public static bool GenerateCameras()
        {
            CurrentCamera = null;
            Cameras = new Dictionary<int, Camera>();
            CameraScreen.ResetScreenCounter();

            // Get Camera Settings
            List<SettingsGeneralCameraSystem> cameraSettingsList = SettingsService.GetCameraSettings();
            
            // Get Image Settings
            List<ImageCamera> imageCameraSettingsList = SettingsService.GetImageCameraSettings();

            // Iterate over image settings list and create camera objects
            foreach (ImageCamera imageCameraSettings in imageCameraSettingsList)
            {
                SettingsGeneralCameraSystem cameraSettings = cameraSettingsList
                    .Where(c => c.CameraId == imageCameraSettings.CameraID)
                    .First();

                if (!imageCameraSettings.Enabled && cameraSettings.Mode != CameraMode.ModeMarker)
                    SettingsService.SetCameraMode(imageCameraSettings.CameraID, cameraSettings.Mode);
                                
                ImageResolution imageResolution = new ImageResolution(imageCameraSettings.Width / 2, imageCameraSettings.Height / 2);

                // Create camera object and add it to dictionary
                Camera camera = new Camera(imageCameraSettings.CameraID, cameraSettings, imageResolution);                
                Cameras.Add(camera.ID, camera);

                // Make sure that the current settings are reflected in the state of the application
                // The state of the QTM host should always have precedence unless expliciltly told to
                // change settings
                if (CurrentCamera == null)
                    CurrentCamera = camera;
            }

            // Load and run profiler
            CameraProfiler cameraProfiler = new CameraProfiler(Cameras, "CameraProfiles.json");
            cameraProfiler.Run();

            // Dispose of it once it's done
            cameraProfiler.Dispose();
            cameraProfiler = null;

            return true;
        }

        public static void GenerateCameraScreens(Urho.Node node)
        {
            Cameras.Values.ToList().ForEach(camera => camera.GenerateScreen(node));
        }

        public static List<Camera> GetCameras()
        {
            return Cameras.Values.ToList();
        }

        // Set the currently selected camera
        public static void SetCurrentCamera(int id)
        {
            // Before setting the new camera make sure to deselect the old one
            CurrentCamera.Deselect();
            CurrentCamera = Cameras[id];
            CurrentCamera.Select();
        }

        public static void RefreshSettings()
        {
            List<SettingsGeneralCameraSystem> settingsList = SettingsService.GetCameraSettings();

            if (settingsList == null)
                return;

            foreach(var settings in settingsList)
            {
                Cameras[settings.CameraId].UpdateSettings(settings);
            }
        }

        public static void Clean()
        {
            CurrentCamera = null;

            if (Cameras != null)
            {
                Cameras.Clear();
                Cameras = null;
            }

            settingsService = null;

            if (Screens != null)
            {
                Screens.Clear();
                Screens = null;
            }                        
        }

        /// <summary>
        /// Enables all camera screens so that they get rendered to the screen
        /// NOTE: The current camera is not modified as it is assumed to already be enabled
        /// </summary>
        /// <param name="enable">bool that decides if the screens should be enabled or not</param>
        public static void EnableCameraScreens(bool enable = true, int selection = -1)
        {
            Cameras.Values.ToList().ForEach((camera) =>
            {
                if ((selection == -1 || camera.ID != selection) && camera.Screen != null)
                {
                    Urho.Application.InvokeOnMain(() =>
                    {
                        
                        if (enable)
                            camera.Screen.Node.ResetDeepEnabled();
                        else
                            camera.Screen.Node.SetDeepEnabled(enable);

                    });
                }
                else
                {
                    Debug.WriteLine(camera.ID);
                }
            });
        }

    }
}
