using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Arqus.Helpers;

namespace Arqus
{
    static class Gestures
    {
        private static Vector3 panOffset = Vector3.Zero;
        private static float pinchPrecision = 0.1f;
        private static float pinchSpeed = 0.5f;
            
        public static void PinchAndZoom(this Urho.Camera camera, TouchState fingerOne, TouchState fingerTwo, float min, float max)
        {
            // Pinching
            if (DataOperations.IsPinching(ref fingerOne.Delta.X, ref fingerTwo.Delta.X, ref fingerOne.Delta.Y, ref fingerTwo.Delta.Y))
            {
                // Get delta distance between both touches
                double oldDistance = DataOperations.GetDistance2D(fingerOne.LastPosition.X, fingerTwo.LastPosition.X, fingerOne.LastPosition.Y, fingerTwo.LastPosition.Y);
                double newDistance = DataOperations.GetDistance2D(fingerOne.Position.X, fingerTwo.Position.X, fingerOne.Position.Y, fingerTwo.Position.Y);
                double deltaDistance = oldDistance - newDistance;
                bool reset2DPosition = false;

                // Precision control
                if (Math.Abs(deltaDistance) > pinchPrecision)
                {
                    float scale = (float)(newDistance / oldDistance);
                    float zoom = (newDistance > oldDistance) ? scale : -scale;
                    zoom *= pinchSpeed;
                    // TODO: transform the precision depending on distance from screen to camera so
                    // that the user can zoom in really closely with great precision
                    float newPosition = camera.Node.Position.Z + zoom;

                    if (newPosition <= min)
                    {                        
                        newPosition = min;

                        // We also need to reset panning (2D position)
                        // TODO: Handle a smooth transition hier
                        reset2DPosition = true;
                    }
                    else if (newPosition >= max)
                    {
                        newPosition = max;
                    }

                    // NOTE: Is this marked as dirty and only updated in the update loop
                    camera.Node.SetWorldPosition(new Vector3(reset2DPosition == true ? 0.0f : camera.Node.Position.X, reset2DPosition == true ? 0.0f : camera.Node.Position.Y, newPosition));                    
                }
            }
        }

        public static void Pan(this Urho.Camera camera, int dx, int dy, float panningSpeed, float zoomThreshold)
        {
            // Check if camera is zoomed in, if so proceed
            if (camera.Node.Position.Z == zoomThreshold)
                return;

            // Update offset 
            panOffset.X = -dx;
            panOffset.Y = dy;

            // Add offset
            camera.Node.Position += panOffset * panningSpeed;            
        }

    }
}
