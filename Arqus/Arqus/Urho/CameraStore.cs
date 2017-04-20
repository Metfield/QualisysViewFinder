using QTMRealTimeSDK;
using Arqus.Visualization;

using System.Linq;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;

namespace Arqus
{    
    /// <summary>
    /// Object that holds the state of the currently selected camera
    /// </summary>
    public class CameraState
    {
        public int ID { get;  set; }
        public CameraMode Mode { get; set; }

        public int MarkerExposure { get; set; }
        public int MarkerThreshold { get; set; }

        public int VideoExposure { get; set; }
        public int VideoFlash { get; set; }

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
        static QTMNetworkConnection connection = new QTMNetworkConnection();
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
        public static List<CameraScreen> GenerateCameraScreens()
        {
            List<CameraScreen> cameraScreens = new List<CameraScreen>();
            CameraScreen.ResetScreenCounter();
            
            List<SettingsGeneralCameraSystem> cameraSettings = SettingsService.GetCameraSettings();
            List<ImageCamera> imageSettings = SettingsService.GetImageCameraSettings();

            foreach (ImageCamera imageCamera in imageSettings)
            {
                CameraMode cameraMode = cameraSettings.Where(camera => camera.CameraId == imageCamera.CameraID).First().Mode;

                if(!imageCamera.Enabled && cameraMode != CameraMode.ModeMarker)
                    SetCameraMode(cameraMode, imageCamera.CameraID);

                bool isImageMode = cameraMode != CameraMode.ModeMarker;

                CameraScreen screen = new CameraScreen(
                    imageCamera.CameraID, 
                    imageCamera.Width, 
                    imageCamera.Height, 
                    isImageMode
                    );

                cameraScreens.Add(screen);

                // Make sure that the current settings are reflected in the state of the application
                // The state of the QTM host should always have precedence unless expliciltly told to
                // change settings
                if (imageCamera.CameraID == State.ID)
                    State.Mode = cameraMode;
                
            }

            return cameraScreens;
        }

        public static async void SetCameraMode(CameraMode mode, int id)
        {
            await SettingsService.SetCameraMode(id, mode);
        }

        public static async void SetCameraMode(CameraMode mode)
        {
            bool success = await SettingsService.SetCameraMode(State.ID, mode);

            if (success)
                State.Mode = mode;
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
