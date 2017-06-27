using Arqus.Services;
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
        public abstract void SetCameraScreenPosition(CameraScreen screen, DeviceOrientations orientation);
        public abstract Camera Camera { get; }

        public abstract void OnTouchBegan(TouchBeginEventArgs eventArgs);
        public abstract void OnTouch(Input input, TouchMoveEventArgs eventArgs);
        public abstract void OnTouchReleased(Input input, TouchEndEventArgs eventArgs);
    }
}
