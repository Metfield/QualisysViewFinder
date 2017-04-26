using QTMRealTimeSDK;
using Arqus.Visualization;

using System.Linq;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;
using Arqus.Helpers;
using Arqus.DataModels;

namespace Arqus
{
    /// <summary>
    /// Object that holds the state of the currently selected camera
    /// </summary>
    public class CameraState
    {
        public int ID { get; set; }
        public CameraMode Mode { get; set; }

        public int NumCams{ get; set; }
        
        public CameraState(int id, CameraMode mode)
        {
            ID = id;
            Mode = mode;
        }
    }

    /// <summary>
    /// Camera store that handles retreival of up-to-date cameras and the current 
    /// state of the cameras in the application
    /// </summary>
    class CameraStore
    {        
        // TODO: Initialize this depending on the first cameras current mode when connecting to the QTM host
        public static CameraState State = new CameraState(1, CameraMode.ModeMarker);

        public static Camera CurrentCamera;
        public static Dictionary<int, Camera> Cameras;
        static SettingsService settingsService = new SettingsService();
        static public List<CameraScreen> Screens { get; set; }
        

        /// <summary>
        /// Name: GenerateCameraScreens
        /// Created: 19-04-2017
        /// 
        /// Generates camera screens to use inside an Urho context based on the 
        /// ImageCameras recieved from the QTM host.
        /// </summary>
        /// <returns></returns>
        public static bool GenerateCameras()
        {
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

                bool isImageMode = cameraSettings.Mode != CameraMode.ModeMarker;

                ImageResolution imageResolution = new ImageResolution(912, 544);
                Camera camera = new Camera(imageCameraSettings.CameraID, cameraSettings.Mode, cameraSettings.MarkerResolution, imageResolution, cameraSettings.Model, cameraSettings.Orientation);
                Cameras.Add(camera.ID, camera);
                
                // Make sure that the current settings are reflected in the state of the application
                // The state of the QTM host should always have precedence unless expliciltly told to
                // change settings
                if (CurrentCamera == null)
                    CurrentCamera = camera;
            }

            return true;
        }

        public static List<CameraScreen> GenerateCameraScreens()
        {
            return Cameras.Values.Select(camera => new CameraScreen(camera)).ToList();
        }

        public static void SetCurrentCamera(int id)
        {
            CurrentCamera = Cameras[id];
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
