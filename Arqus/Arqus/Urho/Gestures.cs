using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Arqus.Helpers;

namespace Arqus
{
    static class Gestures
    {
        
        private static float pinchPrecision = 0.1f;
            
        public static void PinchAndZoom(this Urho.Camera camera, TouchState fingerOne, TouchState fingerTwo, float min, float max)
        {
            // Pinching
            if (DataOperations.IsPinching(ref fingerOne.Delta.X, ref fingerTwo.Delta.X, ref fingerOne.Delta.Y, ref fingerTwo.Delta.Y))
            {
                // Get delta distance between both touches
                double oldDistance = DataOperations.GetDistance2D(fingerOne.LastPosition.X, fingerTwo.LastPosition.X, fingerOne.LastPosition.Y, fingerTwo.LastPosition.Y);
                double newDistance = DataOperations.GetDistance2D(fingerOne.Position.X, fingerTwo.Position.X, fingerOne.Position.Y, fingerTwo.Position.Y);
                double deltaDistance = oldDistance - newDistance;

                // Precision control
                if (Math.Abs(deltaDistance) > pinchPrecision)
                {
                    float scale = (float)(newDistance / oldDistance);
                    float zoom = (newDistance > oldDistance) ? scale : -scale;

                    // Update camera offset
                    if(camera.Node.Position.Z <= min && camera.Node.Position.Z >= max)
                        camera.Node.SetWorldPosition(new Vector3(camera.Node.Position.X, camera.Node.Position.Y, camera.Node.Position.Z + zoom));
                    else if(camera.Node.Position.Z > min)
                        camera.Node.SetWorldPosition(new Vector3(camera.Node.Position.X, camera.Node.Position.Y, min));
                    else
                        camera.Node.SetWorldPosition(new Vector3(camera.Node.Position.X, camera.Node.Position.Y, max));
                }
            }
        }

    }
}
