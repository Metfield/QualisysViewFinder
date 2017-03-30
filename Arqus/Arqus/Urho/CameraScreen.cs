using Arqus.Helpers;
using Arqus.Components;
using Urho;

namespace Arqus.Visualization
{
    /// <summary>
    /// Displays and visualizes 2D streaming data on its children
    /// </summary>
    public class CameraScreen : Component
    {
        public MarkerSpherePool Pool { set; get; }
        static int screenCount;
        public int position;

        public double CenterX { get; set; }
        public double CenterY { get; set; }

        private Node screenNode;
        public int CameraID { private set; get; }
        public ImageResolution Resolution { private set; get; }

        public float Height { private set; get; }
        public float Width { private set; get; }

        public CameraScreen(int cameraID, ImageResolution resolution)
        {
            // Set position according to screenCount and increment the counter
            position = screenCount;
            screenCount++;

            CameraID = cameraID;
            Resolution = resolution;
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            screenNode = node.CreateChild();
            Pool = new MarkerSpherePool(20, node.CreateChild());

            Height = 30;
            Width = Resolution.PixelAspectRatio * Height;

            screenNode.Scale = new Vector3(Width, 0, Height);
            screenNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);

            var frame = screenNode.CreateComponent<Urho.Shapes.Plane>();
            frame.SetMaterial(Material.FromColor(new Color(0f, 0.1f, 0.1f), true));
        }

        protected override void OnUpdate(float timeStep)
        {

            Node[] markerSpheres = screenNode.Parent.GetChildrenWithComponent<MarkerSphere>(true);
            screenNode.Parent.Position = new Vector3((float) CenterX, 0, (float)CenterY);

            foreach (MarkerSphere sphere in markerSpheres[0].Components)
            {
                sphere.markerNode.Scale = new Vector3(sphere.MarkerData.DiameterX / 64.0f * Width / Resolution.Width, sphere.MarkerData.DiameterY / 64.0f * Height / Resolution.Height, sphere.markerNode.Scale.Z);
                sphere.markerNode.Position = new Vector3(sphere.MarkerData.X / 64.0f * Width / Resolution.Width - Width * 0.5f, -sphere.MarkerData.Y / 64.0f * Height / Resolution.Height + Height * 0.5f, sphere.markerNode.Position.Z);
            }
        }
        
    }
}
