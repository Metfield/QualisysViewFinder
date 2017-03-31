using Arqus.Camera2D;
using Arqus.Components;
using Arqus.Visualization;
using System.Collections.Generic;
using System.Diagnostics;
using Urho;

namespace Arqus.Visualization
{
    /// <summary>
    /// Displays and visualizes 2D streaming data on its children
    /// </summary>
    public class CameraScreen : Component
    {
        public MarkerSpherePool Pool { set; get; }
        Node markersRootNode;

        static int screenCount;
        public int position;

        public double CenterX { get; set; }
        public double CenterY { get; set; }

        private Node screenNode;
        public int CameraID { private set; get; }
        public ImageResolution Resolution { private set; get; }

        public float Height { private set; get; }
        public float Width { private set; get; }
        public Color FrameColor { set; get; }

        QTMRealTimeSDK.Data.Camera cameraData;

        /// <summary>
        /// Grid element, contains a frame and the marker spheres to be displayed
        /// </summary>        
        public CameraScreen(int cameraID, ImageResolution resolution, float frameHeight, float frameWidth, Color backgroundColor)
        {
            // Set position according to screenCount and increment the counter
            position = screenCount;
            screenCount++;

            CameraID = cameraID;
            Resolution = resolution;
            ReceiveSceneUpdates = true;

            FrameColor = backgroundColor;

            Height = frameHeight;
            Width = frameWidth;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            // Create screen node, its plane shape and transform it
            screenNode = node.CreateChild("screenNode");
            screenNode.Scale = new Vector3(Width, 0, Height);
            screenNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);

            Urho.Shapes.Plane frame = screenNode.CreateComponent<Urho.Shapes.Plane>();
            frame.SetMaterial(Material.FromColor(FrameColor, true));

            // Create markers pool and initialize with arbitrary size
            Pool = new MarkerSpherePool(20, screenNode);

            // Update stream data
            CameraStream.Instance.UpdateStreamData();
        }
                
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            // Get camera information
            cameraData = CameraStream.Instance.GetCamera2D(CameraID);

            // Get necessary frame information to position markers
            // Horizontal bounds
            float leftBound = screenNode.WorldPosition.X - (Width * 0.5f);
            float rightBound = leftBound + Width;

            // Vertical bounds
            float upperBound = screenNode.WorldPosition.Y + (Height * 0.5f);
            float lowerBound = upperBound - Height;

            // This index will be used as an array pointer to help identify and disable
            // markers which are not being currently used
            int lastUsedInArray = 0;

            // Iterate through the marker array, transform and draw spheres
            for (int i = 0; i < cameraData.MarkerCount; i++)
            {
                // Transform from camera coordinates to frame coordinates
                float adjustedX = Helpers.DataOperations.ConvertRange(0, Resolution.Width, leftBound, rightBound, cameraData.MarkerData2D[i].X / 64);
                float adjustedY = Helpers.DataOperations.ConvertRange(0, Resolution.Height, upperBound, lowerBound, cameraData.MarkerData2D[i].Y / 64);

                // Set world position with new frame coordinates            
                Pool.Get(i).SetWorldPosition(new Vector3(adjustedX, adjustedY, screenNode.WorldPosition.Z - 1));

                // Last element will set this variable
                lastUsedInArray = i;
            }

            // Hide the markers which were not used on this frame
            Pool.HideUnused(lastUsedInArray);
        }
    }
}
