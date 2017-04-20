using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    class CameraSettings
    {
        public int CameraID { get; private set; }

        // Marker/intensity-related
        public int MarkerExposure { get; set; }
        public int MarkerThreshold { get; set; }

        // Video stream-related
        public int VideoExposure { get; set; }
        public int VideoFlash { get; set; }

        public CameraSettings(int id)
        {
            CameraID = id;
        }
    }
}
