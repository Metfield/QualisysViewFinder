using Arqus.Camera2D;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using Urho;

namespace Arqus
{
    class GridViewComponent : Component
    {
        QTMNetworkConnection qtmConnection;
        int cameraCount;
        List<ImageCamera> cameras;

        // Implement dynamic changes hier
        public int Columns { get; set; }
        public Vector3 Origin { get; set; }
        public float FrameHeight { get; set; }
        public float FrameWidth { get; private set; }
        public float Padding { get; set; }

        Color frameColor;        

        public GridViewComponent(Vector3 origin, float frameHeight, int columns, Color _frameColor)
        {
            // Get network connection reference
            qtmConnection = QTMNetworkConnection.Instance;

            // Get camera count from stream class
            cameraCount = CameraStream.Instance.GetStreamMarkerData().Count;

            // Fill column number
            Columns = columns;

            // Color for camera frames
            frameColor = _frameColor;

            // Set grid origin
            Origin = origin;

            // Set frame height
            FrameHeight = frameHeight;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            FillGrid(node);
        }

        private void FillGrid(Node gridNode)
        {
            // Get list of cameras
            cameras = qtmConnection.GetImageSettings();
            Node gridElementNode;
            int currColumn = -1, currRow = 1;
             
            // Create a grid element (cameraScreen) for each one
            foreach(ImageCamera camera in cameras)
            {
                // Create resolution object and calculate frame size
                ImageResolution imageResolution = new ImageResolution(camera.Width, camera.Height);
                FrameWidth = imageResolution.PixelAspectRatio * FrameHeight;

                // Create screen component and node. Add it to parent node (scene)
                Visualization.CameraScreen screen = new Visualization.CameraScreen(camera.CameraID, imageResolution, FrameHeight, FrameWidth, frameColor);
                gridElementNode = gridNode.CreateChild("Camera" + camera.CameraID.ToString());                
                gridElementNode.AddComponent(screen);

                // Determine element's position in grid
                if (++currColumn == Columns)
                {
                    // Reset column
                    currColumn = 0;

                    // Increase row
                    currRow++;
                }
        
                // Calculate position offset and add it 
                Vector3 offset = Vector3.Multiply(new Vector3(currColumn, currRow, 0), new Vector3(FrameWidth + Padding, -FrameHeight - Padding, 0));
                gridElementNode.Position += offset;
            }

            // Fix gridView to position (upper-left corner)
            gridNode.Position = Origin;

            // Add mode to camera stream
        }
    }
}
