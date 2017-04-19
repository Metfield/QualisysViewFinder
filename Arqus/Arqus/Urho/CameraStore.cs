using Arqus.Helpers;
using Arqus.Visualization;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arqus
{
    
    /// <summary>
    /// Object that holds the state of the currently selected camera
    /// </summary>
    public class CameraState
    {
        public int ID { get; set; }
        public CameraMode Mode { get; set; }

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
        //public static CameraState State = new CameraState(1, CameraMode.ModeMarker);
        static QTMNetworkConnection connection = new QTMNetworkConnection();

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
            return connection.GetImageSettings().Select(camera => new CameraScreen(camera.CameraID, camera.Width, camera.Height)).ToList();
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
