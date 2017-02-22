using System;
using System.Globalization;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace Arqus.Visualization
{
    /// <summary>
    /// A dot component with an attached label
    /// </summary>
    public class MarkerSphere : Component
    {
        Node markerNode;
        Node labelNode;
        bool LabelHidden { set; get; }
        Text3D label;
        Color color;

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
            marker.Color = color;

            // Set the position of the node
            /*labelNode = node.CreateChild();
            labelNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
            labelNode.Position = new Vector3(0, 10, 0);
            */

            // Set the label
            /*
            label = labelNode.CreateComponent<Text3D>();
            label.SetFont(Application.ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 60);
            label.TextEffect = TextEffect.Stroke;
            */

            base.OnAttachedToNode(node);
        }

        public void Set2DPosition(Vector2 position)
        {
            markerNode.Position = new Vector3(position.X, position.Y, markerNode.Position.Z);
        }
        
    }
}
