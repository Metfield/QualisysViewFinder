using Arqus.Visualization;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Arqus.Components
{
    class MarkerSpherePool
    {
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

        public void Add(MarkerSphere component)
        {
            markerSpheres.Add(component);
            root.AddComponent(component);
        }

        public MarkerSphere Get(int index)
        {
            // If an object doesn't exists for the current index we create it
            if (markerSpheres.Count <= index)
                Add(new MarkerSphere());

            return markerSpheres[index];
        }
    }
}
