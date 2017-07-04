using System;
using System.Globalization;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;
using QTMRealTimeSDK.Data;
using Arqus.Helpers;

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

        public void Redraw(float x, float y, float width, float height, float clampWidth, float clampHeight, float z = -0.01f)
        {

            for (uint k = 0; k <= Quality + 1; k++)
            {
                float vx = clamp(x + (width * (float)Math.Sin(k * markerAngle)), clampWidth);
                float vy = clamp(y + (height * (float)Math.Cos(k * markerAngle)), clampHeight);

                unsafe
                {
                    CustomGeometryVertex* vertex = GetVertex(0, k);
                    if (vertex != null)
                    {
                        vertex->Position = new Vector3(vx, vy, z);
                    }
                }
            }

            Commit();
        }

        public static float clamp(float value, float clampValue)
        {
            if (value > clampValue)
                return clampValue;
            else if (value < -clampValue)
                return -clampValue;
            return value;
        }


    }
}
