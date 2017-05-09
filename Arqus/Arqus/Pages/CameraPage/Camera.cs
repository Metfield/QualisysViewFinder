using Arqus.Helpers;
using Arqus.Service;
using Arqus.Visualization;
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.DataModels
{
    class ElasticsearchCameraEvent
    {
        public string OldMode { get; set; }
        public string NewMode { get; set; }
        public int CameraID { get; set; }
    }

    public class Camera
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

            SettingsService.EnableImageMode(ID, Mode != CameraMode.ModeMarker, ImageResolution.Width, ImageResolution.Height);
        }

        /// <summary>
        /// Set the camera stream mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(CameraMode mode)
        {
            // Update mode if not already running in that mode
            if(Mode != mode)
            {
                if (SettingsService.SetCameraMode(ID, mode))
                {
                    if (mode != CameraMode.ModeMarker)
                        Enable();
                    else
                        Disable();

                    ElasticsearchCameraEvent cameraEvent = new ElasticsearchCameraEvent()
                    {
                        OldMode = Mode.ToString(),
                        NewMode = mode.ToString(),
                        CameraID = ID
                    };

                    Mode = mode;

                    MessagingService.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + ID, Mode, payload: cameraEvent);
                }
            }
        }
        
        
        public void Select()
        {
            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.On, SettingsService.LEDColor.Amber));
        }

        public void Deselect()
        {
            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.Off));
        }
        
        public void Enable()
        {
            SettingsService.EnableImageMode(ID, true, ImageResolution.Width, ImageResolution.Height);
        }

        public void Disable()
        {
            //SettingsService.EnableImageMode(ID, false, ImageResolution.Width, ImageResolution.Height);
        }
    }
}
