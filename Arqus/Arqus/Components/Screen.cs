using Urho;

namespace Arqus.Visualization
{
    /// <summary>
    /// A Screen component is a plane with a set of children that applies
    /// and visualizes data to its child nodes
    /// </summary>
    public class Screen : Component
    {
        protected Node frameNode;

        public Screen()
        {
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            frameNode = node;
            frameNode.Scale = new Vector3(10, 0, 10);
            frameNode.Position = new Vector3(0, 0, 2);
            frameNode.Rotation = Urho.Quaternion.FromAxisAngle(new Vector3(-1, 0, 0), 90);

            var frame = frameNode.CreateComponent<Urho.Shapes.Plane>();
            frame.Color = new Color(0.5f, 0.5f, 0.5f);
        }

        protected virtual void UpdateChildren()
        {
        }

    }
}
