using Arqus.Helpers;
using Arqus.Service;
using Arqus.Visualization;
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Arqus.DataModels
{
    class Camera
    {
        // public properties
        public QTMRealTimeSDK.Settings.Resolution MarkerResolution { get; private set; }
        public ImageResolution ImageResolution { get; private set; }
        public CameraMode Mode { get; set; }
        public CameraModel Model { get; private set; }
        public int Orientation { get; set; }

        // private variables
        private int id;
        public int ID { get; private set; }
        private CameraMode currentMode;

        public Camera(int id, CameraMode mode, QTMRealTimeSDK.Settings.Resolution markerResolution, ImageResolution imageResolution, CameraModel model, int orientation)
        {
            ID = id;
            Mode = mode;
            Model = model;
            Orientation = orientation;
            ImageResolution = imageResolution;
            MarkerResolution = markerResolution;
        }

        /// <summary>
        /// Set the camera stream mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetCameraMode(CameraMode mode)
        {
            // Update mode if not already running in that mode
            if(Mode != mode)
            {
                if (SettingsService.SetCameraMode(ID, mode))
                    Mode = mode;

                MessagingCenterService.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + ID, Mode);
            }
        }
        
    }
}
