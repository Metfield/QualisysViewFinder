using QTMRealTimeSDK.Settings;
using System.Collections.Generic;

using Urho;
using Arqus.Helpers;
using Arqus.Visualization;
using System.Linq;
using System;
using Arqus.Services;

namespace Arqus
{
    class GridScreenLayout : CameraScreenLayout
    {
        int cameraCount;
        Node gridNode;
        
        public int Columns { get; set; }
        public float Padding { get; set; }
        public Vector3 Origin { get; set; }


        public override int ItemCount { get; set; }
        public override float Offset { get; set; }
        public override Camera Camera { get; }

        public GridScreenLayout(int itemCount, int columns, Urho.Camera camera)
        {
            Columns = columns;
            Camera = camera;
            ItemCount = itemCount;

            // TODO: Handle offset when panning
            Offset = 0;
        }

        public override void Select(int id)
        {
            Selection = id;
        }

        public override void SetCameraScreenPosition(CameraScreen screen, DeviceOrientations orientation)
        {
            float distance = 70;
            float halfHeight = distance * Camera.HalfViewSize;
            float halfWidth = halfHeight * Camera.AspectRatio;

            float margin = 2;
            // Something is off with this algorithm as they are not being centered fully....
            float x = -halfWidth + ( ((Columns - 1) - screen.position % Columns)) * halfWidth * 2 / Columns + halfWidth / Columns;
            float y = halfHeight - (float)Math.Floor((double)(screen.position - 1) / (float)Columns) * (screen.Height + margin) - screen.Height / 2 - margin;
            

            // We need a small offset or the will not be seen by the camera
             screen.Node.SetWorldPosition(new Vector3(x, y, Camera.Node.Position.Z + distance));
        }

        public override void OnTouch(Input input, TouchMoveEventArgs eventArgs)
        {
        }

        public override void OnTouchBegan(TouchBeginEventArgs eventArgs)
        {
        }

        public override void OnTouchReleased(Input input, TouchEndEventArgs eventArgs)
        {
        }
    }
}
