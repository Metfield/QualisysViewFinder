using Arqus.Visualization;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Arqus.Components
{
    /// <summary>
    /// Holds markerSphere nodes; creates more when needed
    /// </summary>
    public class MarkerSpherePool
    {
        // Root node to hold every marker
        private Node root;
        private List<MarkerSphere> markerSpheres;
        public int Count { private set { } get { return markerSpheres.Count; } }

        public MarkerSpherePool(int count, Node rootNode)
        {
            root = rootNode;
            markerSpheres = new List<MarkerSphere>(count);

            for(int index = 0; index < count; index++)
            {
                Add(new MarkerSphere());
            }
        }

        public void Add(MarkerSphere sphereNode)
        {
            markerSpheres.Add(sphereNode);
            root.AddChild(sphereNode);
        }

        public MarkerSphere Get(int index)
        {
            // If an object doesn't exists for the current index we create it
            if (markerSpheres.Count <= index)
                Add(new MarkerSphere());

            markerSpheres[index].Enabled = true;
            return markerSpheres[index];
        }

        public void Hide()
        {
            foreach(var sphere in markerSpheres)
            {
                sphere.Enabled = false;
            }
        }

        // TODO: Look over how the spheres gets hidden since there seems to a couple of markers that stay longer the expected
        public void HideUnused(int startingArrayPosition)
        {
            for (int i = startingArrayPosition; i < markerSpheres.Count; i++)
                markerSpheres[i].Enabled = false;
        }
    }   
}
