using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using Urho;
using Arqus.Helpers;

namespace Arqus
{
    class GridViewComponent : Component
    {
        QTMNetworkConnection networkConnection;
        int cameraCount;
        List<ImageCamera> cameras;
        Node gridNode;

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
            networkConnection = new QTMNetworkConnection();

            // Get camera count from stream class
            //cameraCount = CameraStream.Instance.GetStreamMarkerData().Count;

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

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);                        

            // If there is a change in the amount of connected cameras, re-create the grid
            /*if (cameraCount != CameraStream.Instance.GetStreamMarkerData().Count)
            {
                // Clear the node from any children
                gridNode.RemoveAllChildren();

                // Re-fill it
                FillGrid(gridNode);
            } 
            */
        }

        /// <summary>
        /// Creates the grid structure along with all its elements (camera screens)
        /// </summary>
        /// <param name="node"></param>
        private void FillGrid(Node node)
        {
            // Set Gridnode reference
            gridNode = node;

            // Get list of cameras
            cameras = networkConnection.GetImageSettings();
            Node gridElementNode;
            int currColumn = -1, currRow = 1;
             
            // Create a grid element (cameraScreen) for each one
            foreach(ImageCamera camera in cameras)
            {
                // Create resolution object and calculate frame size
                ImageResolution imageResolution = new ImageResolution(camera.Width, camera.Height);
                FrameWidth = imageResolution.PixelAspectRatio * FrameHeight;

                // Create screen component and node. Add it to parent node (scene)
                // TODO: handle 
                Visualization.CameraScreen screen = new Visualization.CameraScreen(camera.CameraID, imageResolution, FrameHeight, FrameWidth, frameColor, 0);
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
        }
    }
}
