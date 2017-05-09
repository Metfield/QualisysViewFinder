using QTMRealTimeSDK.Settings;
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
            Columns = columns;
            this.camera = camera;
            ItemCount = itemCount;

            // TODO: Handle offset when panning
            Offset = 0;
        }
        

        public override void Select(int id)
        {
            Selection = id;
        }

        public override void SetCameraScreenPosition(CameraScreen screen)
        {
            float distance = 70;
            float halfHeight = distance * camera.HalfViewSize;
            float halfWidth = halfHeight * camera.AspectRatio;

            float margin = 2;
            // Something is off with this algorithm as they are not being centered fully....
            float x = -halfWidth + ( ((Columns - 1) - screen.position % Columns)) * halfWidth * 2 / Columns + halfWidth / Columns;
            float y = halfHeight - (float)Math.Floor((double)(screen.position - 1) / (float)Columns) * (screen.Height + margin) - screen.Height / 2 - margin;
            

            // We need a small offset or the will not be seen by the camera
             screen.Node.SetWorldPosition(new Vector3(x, y, camera.Node.Position.Z + distance));
        }
    }
}
