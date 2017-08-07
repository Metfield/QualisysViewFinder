using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public class CameraProfile
    {
        public string Model { get; set; }
        public bool AutoExposure { get; set; }
        public bool LensControl { get; set; }
        public bool MarkerModeSupport { get; set; }
    }
}
