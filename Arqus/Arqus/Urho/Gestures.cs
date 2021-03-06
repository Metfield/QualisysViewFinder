﻿using System;
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
            
        public static float GetZoomAmountFromPinch(TouchState fingerOne, TouchState fingerTwo)
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
                    zoom *= newDistance < oldDistance ? pinchSpeed * 2.0f : pinchSpeed;
                    return zoom;
                }
            }

            return 0;
        }
        

        public static void PinchAndZoom(this Urho.Camera camera, TouchState fingerOne, TouchState fingerTwo, float min, float max)
        {
            float zoom = GetZoomAmountFromPinch(fingerOne, fingerTwo);

            if(zoom != 0)
            {
                bool reset2DPosition = false;
                // TODO: transform the precision depending on distance from screen to camera so
                // that the user can zoom in really closely with great precision
                float newPosition = camera.Node.Position.Z + (float) zoom;

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
        
        /// <summary>
        /// Pans a camera in both x- and y-direction
        /// </summary>
        /// <param name="camera">the camera to be panned</param>
        /// <param name="dx">the pan value in x-direction</param>
        /// <param name="dy">the pan value in y-direction</param>
        public static void Pan(this Urho.Camera camera, int dx, int dy, float precision = 0.005f, bool onZoom = true, float maxY = -99999, float minY = -99999, float maxX = -99999, float minX = -99999)
        {
            // Only pan if camera is zoomed
            if (!onZoom || camera.Zoom > 1)
            {   
                float x = camera.Node.Position.X + -dx * precision;
                float y = camera.Node.Position.Y + dy * precision;

                // TODO: create an extension method to handle clamping of values
                if (maxY > -99999 && y > maxY)
                    y = maxY;

                if (minY > -99999 && y < minY)
                    y = minY;

                if (maxX > -99999 && x > maxX)
                    x = maxX;

                if (minX > -99999 && x < minX)
                    x = minX;

                Urho.Application.InvokeOnMain(() => camera.Node.SetPosition2D(new Vector2(x, y)));
            }
            else
            {
                // Reset camera position
                camera.Node.Position = new Vector3(0, 0, camera.Node.Position.Z);
            }
        }
        

    }
}
