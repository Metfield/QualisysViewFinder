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

        public int Row => (int) Math.Ceiling((double) ItemCount / Columns);

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
            float margin = 5;
            float width = 10;
            
            //screen.Scale = 1;

            // Calculate the distance where the camera screen width is half the width of the frustrum
            float distance = (float)DataOperations.GetDistanceForFrustrumWidth(screen.Width * Columns + margin, Camera.AspectRatio, Camera.Fov); ;


            if (Math.Abs(screen.Camera.Orientation) == 0 || Math.Abs(screen.Camera.Orientation) == 180)
            {
                //screen.Node.SetScale(1);
            }
            else
            {
                //float newScale = screen.Height / screen.Width;
                //screen.Scale = newScale;
            }

            float halfHeight = distance * Camera.HalfViewSize;
            float halfWidth = halfHeight * Camera.AspectRatio;


            // Something is off with this algorithm as they are not being centered fully....
            float x = -halfWidth + (((Columns - 1) - screen.position % Columns)) * halfWidth * 2 / Columns + halfWidth / Columns;
            float y = halfHeight - screen.Height / 2 - (float)Math.Floor((double)(screen.position - 1) / (float)Columns) * (screen.Height + margin / 2) - margin / 2;


            if ((y - screen.Height / 2) < min)
                min = y - screen.Height / 2;

            // We need a small offset or the will not be seen by the camera
            screen.Node.SetWorldPosition(new Vector3(x, y, distance));
        }

        float min;

        public override void OnTouch(Input input, TouchMoveEventArgs eventArgs)
        {
            if(input.NumTouches == 1)
            {
                // Only move in y axis
                Camera.Pan(0, eventArgs.DY, 0.05f, false, 0, min);
            }
        }

        public override void OnTouchBegan(TouchBeginEventArgs eventArgs)
        {
        }

        public override void OnTouchReleased(Input input, TouchEndEventArgs eventArgs)
        {

        }
    }
}
