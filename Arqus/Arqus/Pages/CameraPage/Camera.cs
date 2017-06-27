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
        
        public int ID { get; private set; }

        // public properties
        public ImageResolution ImageResolution { get; set; }
        public CameraScreen Parent { get; set; }

        public bool LensControlEnabled { get; private set; }

        public string Model { get; private set; }
        public CameraProfile Profile { get; set; }
            
        public int Orientation { get; private set; }

        private CameraMode mode;
        public CameraMode Mode { get { return mode; } set { SetProperty(ref mode, value); } }

        private SettingsGeneralCameraSystem settings;
        public SettingsGeneralCameraSystem Settings
        {
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
            Model = GetModelName(settings.Model);
            Orientation = settings.Orientation;

            LensControlEnabled = CheckForLensControl();

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

                    if (IsImageMode())
                    {
                        EnableImageMode();
                    }

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
        /// Select the camera
        /// </summary>
        public void Select()
        {
            // Enable image mode if neccessary
            if (IsImageMode())
                EnableImageMode();
            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.On, SettingsService.LEDColor.Amber));
        }
        public void Deselect()
        {
            // Disable image mode when not selected to keep the transferred data to a minimum
            // TODO: Fix QTM 16 crash 
            //DisableImageMode();
            Task.Run(() => SettingsService.SetLED(ID, SettingsService.LEDMode.Off));
        }

        /// <summary>
        /// Disable image mode for streaming on the QTM host
        /// </summary>
        public void DisableImageMode()
        {
           SettingsService.DisableImageMode(ID);
        }

        /// <summary>
        /// Enable image mode for streaming on the QTM host
        /// </summary>
        public void EnableImageMode()
        {
            SettingsService.EnableImageMode(ID, true, ImageResolution.Width, ImageResolution.Height);
        }
        
        /// <summary>
        /// Check to see if the camera is in image mode, such as video or intensity
        /// </summary>
        /// <returns>true if the camera is running in image mode</returns>
        public bool IsImageMode()
        {
            return Mode != CameraMode.ModeMarker;
        }

        // MaxFocus is a bound property by CameraPage.xaml
        // MaxAperture is handled by the LensApertureSnapper class
        public float MaxFocus { get; set; }
        
        // Checks if camera can handle Lens Control; if it does, assign
        // the original camera value to this variable
        private bool CheckForLensControl()
        {
            if (settings.LensControl.Focus.Max != 0 || settings.LensControl.Focus.Min != 0 ||
                settings.LensControl.Focus.Value != 0)
            {
                MaxFocus = Settings.LensControl.Focus.Max;
                return true;
            }

            // Avoid XAML crash when there are no lens control values and 
            // MAX and MIN end up being 0 (which causes an exception)            
            MaxFocus = 1;
            return false;
        }
        
        // Gets model string
        // TODO: Keep an eye out for changes hier
        private string GetModelName(CameraModel cameraModel)
        {
            switch (cameraModel)
            {
                case CameraModel.ModelQqus100:
                    return "Oqus 100 ";

                case CameraModel.ModelQqus200C:
                    return "Oqus 200 C";

                case CameraModel.ModelQqus300:
                    return "Oqus 300";

                case CameraModel.ModelQqus300Plus:
                    return "Oqus 300 Plus";

                case CameraModel.ModelQqus400:
                    return "Oqus 400";

                case CameraModel.ModelQqus500:
                    return "Oqus 500";

                case CameraModel.ModelQqus500Plus:
                    return "Oqus 500 Plus";

                case CameraModel.ModelQqus700:
                    return "Oqus 700";

                case CameraModel.ModelQqus700Plus:
                    return "Oqus 700 Plus";

                case CameraModel.ModelMiqusM1:
                    return "Miqus M1";

                case CameraModel.ModelMiqusM3:
                    return "Miqus M3";

                case CameraModel.ModelMiqusM5:
                    return "Miqus M3";

                case CameraModel.ModelMiqusVideo:
                    return "Miqus Video";

                case CameraModel.ModelMiqusSU:
                    return "Miqus Sync Unit";

                default:
                    return "Model Unknown";
            }
        }
    }
}
