using Urho;

namespace Arqus.Visualization
{
    /// <summary>
    /// A Screen component is a plane with a set of children that applies
    /// and visualizes data to its child nodes
    /// </summary>
    public class Screen : Component
    {
        

        public Screen()
        {
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {

        }

        protected virtual void UpdateChildren() { }

    }
}
