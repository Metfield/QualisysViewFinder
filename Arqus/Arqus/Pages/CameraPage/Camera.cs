using System.Diagnostics;
using System.Threading.Tasks;

using Arqus.Helpers;
using Arqus.Service;
using Arqus.Visualization;

using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;

using Prism.Mvvm;

namespace Arqus.DataModels
{
    /// <summary>
    /// This camera class is supposed to be a model that reflects
    /// the state of the cameras inside the QTM host. Anything that
    /// changes in this model should also be updated on the QTM host.
    /// The purpose of this class is to act as a middleware between
    /// QTM and the Arqus application to properly update values.
    /// </summary>
    public class Camera : BindableBase
    {
        public ImageResolution ImageResolution { get; private set; }

        public CameraScreen Parent { get; set; }
        
        public int ID { get; private set; }

        public int Orientation { get; set; }

        private CameraMode mode;
        public CameraMode Mode { get { return mode; } set { SetProperty(ref mode, value); } }

        private SettingsGeneralCameraSystem settings;
        public SettingsGeneralCameraSystem Settings {
            get { return settings; }
            set
            {
                SetProperty(ref settings, value);
            }
        }
        
        public Camera(int id, SettingsGeneralCameraSystem settings, ImageResolution imageResolution)
        {
            ID = id;
            // TODO: this is part of the settings
            Mode = settings.Mode;
            Settings = settings;
            ImageResolution = imageResolution;

            // TODO: this should not have to be done in the constructor
            if (IsImageMode())
                EnableImageMode();
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
                    Mode = mode;
                    MessagingService.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + ID, Mode);
                }
            }
        }
        
        /// <summary>
        /// Attempts to update settings of the camera by issuing a request to the QTM host
        /// </summary>
        public void UpdateSettings()
        {
            Debug.WriteLine("Updating settings for camera");
            UpdateSettings(SettingsService.GetCameraSettings(ID));
        }

        /// <summary>
        /// Update the settings of the camera if the QTM host updated the settings successfully
        /// </summary>
        /// <param name="settings"></param>
        private void UpdateSettings(SettingsGeneralCameraSystem? settings)
        {
            if (settings != null)
            {
                Settings = settings.Value;
            }
        }

        /// <summary>
        /// Set a certain setting for this camera
        /// </summary>
        /// <param name="setting">the settings to be set</param>
        /// <param name="value">the value to set the setting to</param>
        public void SetSetting(string setting, double value)
        {
            if (SettingsService.SetCameraSettings(ID, setting, value.ToString()))
                UpdateSettings();
        }
        
        /// <summary>
        /// Select the camera and enable the led-ring
        /// </summary>
        public void Select()
        {
            // Enable image mode if neccessary
            if(IsImageMode())
                EnableImageMode();
            

            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.On, SettingsService.LEDColor.Amber));
        }
        
        /// <summary>
        /// Deselects the camera and disables the led-ring
        /// </summary>
        public void Deselect()
        {
            // Disable image mode when not selected to keep the transferred data to a minimum
            DisableImageMode();

            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.Off));
        }
        
        /// <summary>
        /// Enable image mode for streaming on the QTM host
        /// </summary>
        public void EnableImageMode()
        {
            SettingsService.EnableImageMode(ID, true, ImageResolution.Width, ImageResolution.Height);
        }
        
        /// <summary>
        /// Disable image mode for streaming on the QTM host
        /// </summary>
        public void DisableImageMode()
        {
            SettingsService.EnableImageMode(ID, false, ImageResolution.Width, ImageResolution.Height);
        }
        
        /// <summary>
        /// Check to see if the camera is in image mode, such as video or intensity
        /// </summary>
        /// <returns>true if the camera is running in image mode</returns>
        private bool IsImageMode()
        {
            return Mode != CameraMode.ModeMarker;
        }
    }

}

