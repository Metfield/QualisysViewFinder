using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;


namespace Arqus
{
    public class CameraSettings
    {
        public int CameraID { get; private set; }

        // Marker/intensity-related
        public CameraSetting MarkerExposure     { get; set; }
        public CameraSetting MarkerThreshold    { get; set; }

        // Video stream-related
        public CameraSetting VideoExposure      { get; set; }
        public CameraSetting VideoFlashTime     { get; set; }

        public CameraSettings(int id)
        {
            CameraID = id;
        }
    }
}
