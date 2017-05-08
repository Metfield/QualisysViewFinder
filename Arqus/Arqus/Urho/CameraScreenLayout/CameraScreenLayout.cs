using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Arqus.Visualization
{
    public abstract class CameraScreenLayout
    {
        public int Selection { get; set; }
        public abstract void Select(int id);
        public abstract int ItemCount { get; set; }
        public abstract float Offset { get; set; }
        public abstract void SetCameraScreenPosition(CameraScreen screen);
    }
}
