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
        public Grid(Vector3 origin, int itemCount, int columns, Urho.Camera camera)
        {
            // Fill column number
            Columns = columns;
            this.camera = camera;

            // Set grid origin
            Origin = origin;

            ItemCount = itemCount;

            Offset = 0;
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
            Node gridElementNode;
            int currColumn = -1, currRow = 1;

            // Used as origin for lower collision bound
            float lowerCollisionBound = 0;
            
            // Create a grid element (cameraScreen) for each one
            foreach (CameraScreen screen in screens)
            {
                // TODO: get resolution from camera
                //screen.Scale = ScreenScale;

                gridElementNode = gridNode.CreateChild("Camera" + screen.Camera.ID.ToString());                
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
                Vector3 offset = Vector3.Multiply(new Vector3(currColumn, currRow, 0), new Vector3(screen.Width + Padding, -screen.Height - Padding, 0));
                gridElementNode.Position += offset;

                // Set lower collision bound
                lowerCollisionBound = gridElementNode.Position.Y;
            }

            // Fix gridView to position (upper-left corner)
            gridNode.Position = Origin;

            // Add another offset to place collision plane below last element
            //lowerCollisionBound -= (ScreenScale - Padding) * 2.5f;

            // Create collision plane and its node
            Node collisionPlaneNode = gridNode.CreateChild("lowerBounds");
            collisionPlaneNode.Scale = new Vector3(100, 0, 100);
            collisionPlaneNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            //collisionPlaneNode.Position = new Vector3(gridNode.Position.X + FrameWidth, lowerCollisionBound, gridNode.Position.Z);

            Urho.Shapes.Plane collisionPlane = collisionPlaneNode.CreateComponent<Urho.Shapes.Plane>();

            // Make it invisible
            collisionPlane.Color = Color.Transparent;
        }

        public override void Select(int id){ }

        public override void SetCameraScreenPosition(CameraScreen screen)
        {
            // TODO: Document this and fix the algorithm so it works correctly => centering the camera screens as expected
            float FarFrustrumWidth = Math.Abs(camera.Frustum.Vertices[4].X - camera.Frustum.Vertices[7].X);
            float FarFrustrumHeight = Math.Abs(camera.Frustum.Vertices[4].Y - camera.Frustum.Vertices[5].Y);

            // Something is off with this algorithm as they are not being centered fully....
            float x = camera.Frustum.Vertices[7].X + ( ((Columns - 1) - screen.position % Columns)) * FarFrustrumWidth / Columns + FarFrustrumWidth / Columns / 2;
            float y = camera.Frustum.Vertices[7].Y - (float)Math.Ceiling((double)screen.position / Columns) * FarFrustrumHeight / 5;

            // We need a small offset or the will not be seen by the camera
            screen.Node.SetWorldPosition(new Vector3(x, y, camera.Frustum.Vertices[7].Z - 10));
        }
    }
}
