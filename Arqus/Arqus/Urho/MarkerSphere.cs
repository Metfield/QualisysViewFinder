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
    /// Marker node with an attached sphere component
    /// </summary>
    public class MarkerSphere : CustomGeometry
    {
        public Color SphereColor { get; set; }
        public uint Quality { get; set; }

        public MarkerSphere()
        {
            Quality = 40;
            // Set default color to white
            SphereColor = Color.White;

            // Initialize disabled
            Enabled = false;

            // HACK: Arbitrary number of 0.1 makes sense right now
            //SetScale(0.1f);

            // Create sphere component and attach it 
            SetMaterial(Material.FromColor(SphereColor, true));

            DefineGeometry(0, PrimitiveType.TriangleFan, Quality, true, true, false, false);
        }   
    }
}
