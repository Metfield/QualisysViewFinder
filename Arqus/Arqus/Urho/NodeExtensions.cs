using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Visualization
{
    static class NodeExtensions
    {
        public static float Distance(this Urho.Node startNode, Urho.Node endNode)
        {
            return Urho.Vector3.Distance(startNode.Position, endNode.Position);
        }
    }
}
