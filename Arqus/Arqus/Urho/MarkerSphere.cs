using System;
using System.Globalization;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;
using QTMRealTimeSDK.Data;

namespace Arqus.Visualization
{
    /// <summary>
    /// A dot component with an attached label
    /// </summary>
    public class MarkerSphere : Component
    {
        public Node markerNode;
        Node labelNode;
        bool LabelHidden { set; get; }
        Text3D label;
        Color color;
        public bool updated;
        private Q2D markerData;

        public Q2D MarkerData
        {
            set
            {
                updated = true;
                markerData = value;
            }
            get
            {
                return markerData;
            }
        }

        public MarkerSphere()
        {
            this.color = Color.Magenta;
            LabelHidden = false;
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            markerNode = node.CreateChild();
            markerNode.Position = Vector3.Zero;
            markerNode.Scale = new Vector3(1.0f, 1.0f, 1.0f);

            var marker = markerNode.CreateComponent<Sphere>();
            marker.SetMaterial(Material.FromColor(Color.White, true));

            base.OnAttachedToNode(node);
        }

        

        public void Set2DPosition(Vector2 position)
        {
            markerNode.Position = new Vector3(position.X, position.Y, markerNode.Position.Z);
        }

        public void Hide()
        {
            markerNode.Enabled = false;
        }
    }
}
