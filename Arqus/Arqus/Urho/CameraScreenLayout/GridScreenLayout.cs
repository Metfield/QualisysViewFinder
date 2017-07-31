﻿using QTMRealTimeSDK.Settings;
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
            // Prevent the camera from being zoomed when in grid view
            if (Camera.Zoom > 1)
                Camera.Zoom = 1;

            if (Camera.Node.Position.X != 0 || Camera.Node.Position.Y > 0)
                Camera.Node.SetPosition2D(new Vector2(0, 0));

            float margin = 5;
            
            // Calculate the distance where the camera screen width is half the width of the frustrum
            float distance = (float)DataOperations.GetDistanceForFrustrumWidth(screen.Width * Columns + margin, Camera.AspectRatio, Camera.Fov); ;
            
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
