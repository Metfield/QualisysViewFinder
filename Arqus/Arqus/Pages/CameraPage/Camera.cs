using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Pages
{
    class Camera
    {
        // public properties
        public int Width { get; set; }
        public int Height { get; set; }

        // private variables
        private int id;
        private CameraMode currentMode;

        public Camera(int id, int width, int height)
        {
            this.id = id;

            Width = width;
            Height = height;
        }

        /// <summary>
        /// Set the stream mode for the camera
        /// </summary>
        /// <param name="mode"></param>
        public void SetStreamMode(CameraMode mode)
        {
            // Update mode if not already running in that mode
            if(mode != currentMode)
            {
                       
            }
        }
        
    }
}
