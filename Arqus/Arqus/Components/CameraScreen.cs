using Arqus.Visualization;
using System.Diagnostics;
using Urho;

namespace Arqus.Visualization
{
    /// <summary>
    /// Displays and visualizes 2D streaming data on its children
    /// </summary>
    public class CameraScreen : Screen
    {
        public int CameraID { private set; get; }

        public void Init(int cameraID)
        {
            CameraID = cameraID;
        }

        protected override void OnUpdate(float timeStep)
        {
            /*
            Node[] children = frameNode.GetChildrenWithComponent<MarkerSphere>();
            // Get QTM 2D stream data and apply to children dots
            
            foreach (Node child in children)
            {
                Debug.WriteLine("Updating child");
                child.Position = new Vector3(child.Position.X + 0.01f, child.Position.Y, child.Position.Z);
            }
            */

        }
    }
}
