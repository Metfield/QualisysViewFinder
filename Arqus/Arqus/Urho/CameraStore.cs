using QTMRealTimeSDK;
using Arqus.Visualization;

using System.Linq;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;
using Arqus.Helpers;
using Arqus.DataModels;
using Prism.Mvvm;

namespace Arqus
{

    /// <summary>
    /// Camera store that handles retreival of up-to-date cameras and the current 
    /// state of the cameras in the application
    /// </summary>
    class CameraStore : BindableBase
    {
        public static Camera CurrentCamera;
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
            
            List<SettingsGeneralCameraSystem> cameraSettingsList = SettingsService.GetCameraSettings();
            List<ImageCamera> imageCameraSettingsList = SettingsService.GetImageCameraSettings();

            foreach (ImageCamera imageCameraSettings in imageCameraSettingsList)
            {
                SettingsGeneralCameraSystem cameraSettings = cameraSettingsList
                    .Where(c => c.CameraId == imageCameraSettings.CameraID)
                    .First();

                if(!imageCameraSettings.Enabled && cameraSettings.Mode != CameraMode.ModeMarker)
                    SettingsService.SetCameraMode(imageCameraSettings.CameraID, cameraSettings.Mode);

                
                ImageResolution imageResolution = new ImageResolution(imageCameraSettings.Width / 4, imageCameraSettings.Height / 4);
                Camera camera = new Camera(imageCameraSettings.CameraID, cameraSettings, imageResolution);
                Cameras.Add(camera.ID, camera);
                
                // Make sure that the current settings are reflected in the state of the application
                // The state of the QTM host should always have precedence unless expliciltly told to
                // change settings
                if (CurrentCamera == null)
                    CurrentCamera = camera;
            }

            return true;
        }

        public static List<CameraScreen> GenerateCameraScreens(Urho.Node cameraNode)
        {
            return Cameras.Values.Select(camera => new CameraScreen(camera, cameraNode)).ToList();
        }

        public static void SetCurrentCamera(int id)
        {
            CurrentCamera.Deselect();
            CurrentCamera = Cameras[id];
            CurrentCamera.Select();
        }
        

        // TODO: Look over this later
        /*static void RefreshCameraAndScreen()
        {
            connection.GetImageSettings()
                .Where(camera => camera.CameraID == State.ID)
                .Select(camera => new CameraScreen(camera.CameraID, camera.Width, camera.Height))
                .ToList();
        }*/

        //public static void SelectCamera(int id){ State.ID = id; }
        //public static void SelectCamera(int id, CameraMode mode) { State.ID = id; State.Mode = mode; }
    }
}
