using QTMRealTimeSDK.Settings;
using System.Collections.Generic;

using Urho;
using Arqus.Helpers;
using Arqus.Visualization;
using System.Linq;

namespace Arqus
{
    class GridViewComponent : Component
    {
        QTMNetworkConnection networkConnection;
        int cameraCount;
        public List<CameraScreen> screens;
        List<ImageCamera> cameras;
        Node gridNode;

        // Implement dynamic changes hier
        public int Columns { get; set; }
        public Vector3 Origin { get; set; }
        public float ScreenScale { get; set; }
        public float FrameWidth { get; private set; }
        public float Padding { get; set; }

        Color frameColor;
        private CameraStore cameraStore;


        /*
         * 
         */
        public GridViewComponent(Vector3 origin, float frameHeight, int columns, Color _frameColor)
        {

            // Get camera count from stream class
            //cameraCount = CameraStream.Instance.GetStreamMarkerData().Count;

            screens = CameraStore.GenerateCameraScreens();

            // Fill column number
            Columns = columns;

            // Color for camera frames
            frameColor = _frameColor;

            // Set grid origin
            Origin = origin;

            // Set frame height
            ScreenScale = frameHeight;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            FillGrid(node);
        }

        // TODO: Handle change in number of cameras!!
        /*protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);                        

            // If there is a change in the amount of connected cameras, re-create the grid
            if (cameraCount != CameraStream.Instance.GetStreamMarkerData().Count)
            {
                // Clear the node from any children
                gridNode.RemoveAllChildren();

                // Re-fill it
                FillGrid(gridNode);
            } 
            
        }*/

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
                screen.Scale = ScreenScale;

                gridElementNode = gridNode.CreateChild("Camera" + screen.CameraID.ToString());                
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
            lowerCollisionBound -= (ScreenScale - Padding) * 2.5f;

            // Create collision plane and its node
            Node collisionPlaneNode = gridNode.CreateChild("lowerBounds");
            collisionPlaneNode.Scale = new Vector3(100, 0, 100);
            collisionPlaneNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            collisionPlaneNode.Position = new Vector3(gridNode.Position.X + FrameWidth, lowerCollisionBound, gridNode.Position.Z);

            Urho.Shapes.Plane collisionPlane = collisionPlaneNode.CreateComponent<Urho.Shapes.Plane>();

            // Make it invisible
            collisionPlane.Color = Color.Transparent;
        }
    }
}
