﻿using QTMRealTimeSDK.Settings;
using System.Collections.Generic;

using Urho;
using Arqus.Helpers;
using Arqus.Visualization;
using System.Linq;
using System;

namespace Arqus
{
    class Grid : CameraScreenLayout
    {
        int cameraCount;
        public List<CameraScreen> screens;
        List<ImageCamera> cameras;
        Node gridNode;

        // Implement dynamic changes hier
        public int Columns { get; set; }
        public float Padding { get; set; }
        public Vector3 Origin { get; set; }


        public override int ItemCount { get; set; }
        public override float Offset { get; set; }

        private Urho.Camera camera;
            
        /*
         * 
         */
        public Grid(int itemCount, int columns, Urho.Camera camera)
        {
            // Fill column number
            Columns = columns;
            this.camera = camera;
            
            ItemCount = itemCount;

            Offset = 0;
        }
        

        public override void Select(int id){ }

        public override void SetCameraScreenPosition(CameraScreen screen)
        {
            float distance = 70;
            float FrustrumHeight = distance * camera.HalfViewSize;
            float FrustrumWidth = FrustrumHeight * camera.AspectRatio;

            float margin = 5;
            // Something is off with this algorithm as they are not being centered fully....
            float x = -FrustrumWidth + ( ((Columns - 1) - screen.position % Columns)) * FrustrumWidth * 2 / (float) Columns + screen.Width / 2 + FrustrumWidth / (Columns * 2);
            float y = FrustrumHeight - (float)Math.Floor((double)(screen.position - 1) / (float)Columns) * (screen.Height + margin) - screen.Height / 2;
            

            // We need a small offset or the will not be seen by the camera
             screen.Node.SetWorldPosition(new Vector3(x, y, camera.Node.Position.Z + distance));
        }
    }
}
