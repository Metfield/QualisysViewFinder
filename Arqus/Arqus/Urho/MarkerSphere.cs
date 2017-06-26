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
            markerAngle = 2 * Math.PI / Quality;

            // Set default color to white
            SphereColor = Color.White;

            // Initialize disabled
            Enabled = false;

            // HACK: Arbitrary number of 0.1 makes sense right now
            //SetScale(0.1f);

            // Create sphere component and attach it 
            SetMaterial(Material.FromColor(SphereColor, true));

            DefineGeometry(0, PrimitiveType.TriangleFan, Quality, false, true, false, false);
        }

        private double markerAngle;

        public void Redraw(float halfWidth, float halfHeight, float x, float y, float width, float height)
        {

            for (uint k = 0; k <= Quality + 1; k++)
            {
                float a = x + (width * (float)Math.Sin(k * markerAngle));
                float b = y + (height * (float)Math.Cos(k * markerAngle));

                if (a > halfWidth)
                    a = halfWidth;
                else if (a < -halfWidth)
                    a = -halfWidth;

                if (b > halfHeight)
                    b = halfHeight;
                else if (b < -halfHeight)
                    b = -halfHeight;


                unsafe
                {
                    CustomGeometryVertex* vertex = GetVertex(0, k);
                    if (vertex != null)
                    {
                        vertex->Position = new Vector3(a, b, 0);
                    }
                }
            }

            Commit();

        }
    }
}
