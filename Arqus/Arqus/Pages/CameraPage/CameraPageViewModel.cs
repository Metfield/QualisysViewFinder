using Arqus.Helpers;
using Arqus.Service;
using Arqus.Services.MobileCenterService;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    public class CameraPageViewModel : BindableBase, INavigationAware
    {
        private bool isGridLayoutActive;

        public bool IsGridLayoutActive
        {
            get
            {
                return isGridLayoutActive;
            }
            set
            {
                SetProperty(ref isGridLayoutActive, value);
            }
        }

        private INavigationService navigationService;
        
        private CameraPage cameraPageModel;

        private CameraSettingsDrawer settingsDrawer;
        private List<CameraSettings> cameraSettings;

        // Hold slider min and max values
        private const float SLIDER_MIN = 0.0f,
                            SLIDER_MAX = 1.0f;

        bool isChangingMode = false;

        // This will hold the current, temporary camera settings
        List<QTMRealTimeSDK.Settings.SettingsGeneralCameraSystem> tempGeneralSettings;

        // Used to determine if whether Arqus should send new
        // camera setting value to QTM or not. We don't want to
        // send it to QTM if we just got it from it...
        public bool qtmUpdatedSettingValue = true;        

        public DelegateCommand GetStreamDataCommand { get; set; }
        public DelegateCommand SetCameraModeToMarkerCommand { get; set; }
        public DelegateCommand SetCameraModeToVideoCommand { get; set; }
        public DelegateCommand SetCameraModeToIntensityCommand { get; set; }
        public DelegateCommand OnAppearingCommand { get; set; }

        public static Dictionary<string, string> Icon = new Dictionary<string, string>()
        {
            { "grid", "drawable-hdpi/ic_grid_on_black_24dp.png" },
            { "carousel", "drawable-hdpi/ic_view_carousel_black_24dp.png" }
        };

        public DelegateCommand SetCameraScreenLayoutCommand { get; set; }
        private string cameraScreenLayoutIcon = Icon["grid"];
        public string CameraScreenLayoutIcon
        {
            get
            {
                return cameraScreenLayoutIcon;
            }
            set
            {
                SetProperty(ref cameraScreenLayoutIcon, value);
            }
        }

        // TODO: Decouple the View from the ViewModel
        Frame videoDrawerFrame, markerDrawerFrame;

        public CameraPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));

            SetCameraScreenLayoutCommand = new DelegateCommand(() =>
            {    
                // Hide/show drawer according to mode
                // We don't want to show any drawers in grid mode
                if(isGridLayoutActive)
                {
                    IsGridLayoutActive = false;
                    ShowDrawer();
                }
                else
                {
                    IsGridLayoutActive = true;
                    HideDrawer();
                }                    

                MessagingService.Send(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT);
            });

            // We're starting with carousel mode
            isGridLayoutActive = false;

            // NOTE: This couples the ViewModel to the Urho View
            // maybe it's a better idea to create a service which
            // handles selection. If there is a good way to inject
            // the settings service into the Urho view that might
            // be a better way to go about it..
            MessagingCenter.Subscribe<CameraApplication, int>(this,
                MessageSubject.SET_CAMERA_SELECTION.ToString(),
                OnCameraSelection);
                        
            MessagingCenter.Subscribe<QTMEventListener>(this, 
                MessageSubject.CAMERA_SETTINGS_CHANGED.ToString(), 
                UpdateCameraSettings);
            
            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CameraStore.CurrentCamera.ID);            

            // Create camera settings array
            cameraSettings = new List<CameraSettings>();
            // Get latest camera settings
            tempGeneralSettings = SettingsService.GetCameraSettings();

            // Create each camera settings object with a camera id
            for (int i = 1; i <= SettingsService.GetCameraCount(); i++)
            {
                // Create camera settings object
                CameraSettings camSettings = new CameraSettings(i);
                               
                // Copy relevant values 
                camSettings.MarkerExposure = tempGeneralSettings[i - 1].MarkerExposure;  
                camSettings.MarkerThreshold = tempGeneralSettings[i - 1].MarkerThreshold;
                camSettings.VideoExposure = tempGeneralSettings[i - 1].VideoExposure;
                camSettings.VideoFlashTime = tempGeneralSettings[i - 1].VideoFlashTime;

                // Add object to list
                cameraSettings.Add(camSettings);
            }                        

            // Create Camera Settings Drawer object
            settingsDrawer = new CameraSettingsDrawer(this, cameraSettings[CameraStore.CurrentCamera.ID - 1]);

            // Reset flag
            qtmUpdatedSettingValue = false;
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedFrom");

            try
            {
                NavigationMode navigationMode = parameters.GetValue<NavigationMode>("NavigationMode");

                if (navigationMode == NavigationMode.Back)
                    MessagingCenter.Send(Application.Current, MessageSubject.DISCONNECTED);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedTo");
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            MessagingService.Send(Application.Current, MessageSubject.CONNECTED, payload: new { Poop = "poop" });
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            // Set current camera
            CameraStore.SetCurrentCamera(cameraID);

            // Update camera settings and drawer
            UpdateCameraSettings(this);
            
            // Check if camera selection was done through grid mode
            if (IsGridLayoutActive)
            {
                IsGridLayoutActive = false;

                // Invoke on main thread to avoid exception
                Device.BeginInvokeOnMainThread(() => SwitchDrawers(CameraStore.CurrentCamera.Mode));

                return;
            }

            // Switch drawer mode
            Device.BeginInvokeOnMainThread(() => SwitchDrawers(CameraStore.CurrentCamera.Mode));
        }

        private void SetCameraMode(CameraMode mode)
        {           
            // Set the mode
            MobileCenterService.TrackEvent(GetType().Name, "SetCameraMode " + mode.ToString());
            CameraStore.CurrentCamera.SetMode(mode);

            // Switch drawer scheme
            SwitchDrawers(CameraStore.CurrentCamera.Mode);
        }

        public CameraSettingsDrawer GetSettingsDrawer()
        {
            return settingsDrawer;
        }

        /// <summary>
        /// Will be called from the Stream class every time an event
        /// of type "CameraSettingsChanged" happens
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateCameraSettings(Object sender)
        {
            qtmUpdatedSettingValue = true;
            int camIndex = CameraStore.CurrentCamera.ID - 1;

            // Get new camera settings            
            tempGeneralSettings = SettingsService.GetCameraSettings();

            // Copy new values
            cameraSettings[camIndex].MarkerExposure = tempGeneralSettings[camIndex].MarkerExposure;
            cameraSettings[camIndex].MarkerThreshold = tempGeneralSettings[camIndex].MarkerThreshold;
            cameraSettings[camIndex].VideoExposure = tempGeneralSettings[camIndex].VideoExposure;
            cameraSettings[camIndex].VideoFlashTime = tempGeneralSettings[camIndex].VideoFlashTime;           
            
            // Send new settings to drawer 
            settingsDrawer.SetCamera(cameraSettings[camIndex]);
                      
            // Reset flag
            qtmUpdatedSettingValue = false;
        }

        /// <summary>
        /// All these values will temporarily hold sliders-related
        /// information
        /// 
        /// From this point everything concerns drawer menu and sliders.
        /// </summary>                  

        private double markerExposureSliderValue,
                        markerExposureSliderMax,
                        markerExposureSliderMin,
                        markerThresholdSliderValue,
                        markerThresholdSliderMax,
                        markerThresholdSliderMin,

                        videoExposureSliderValue,
                        videoExposureSliderMax,
                        videoExposureSliderMin,
                        videoFlashSliderValue,
                        videoFlashSliderMax,
                        videoFlashSliderMin;

        private void SendCameraSettingValue(string setting, double value)
        {
            // Run this on separate thread to keep UI responsive
            Task.Run(() => SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, setting, (float)value));
        }

        // Command handlers for cammera settings     
        public double MarkerExposureSliderValue
        {
            get { return markerExposureSliderValue; }
            set
            {
                if (markerExposureSliderValue != value)
                {
                    SetProperty(ref markerExposureSliderValue, value);

                    // Set label value                    
                    MarkerExposureValueLabel = Convert.ToInt32(value).ToString();                        

                    // Send it to QTM if value was set locally
                    if (!qtmUpdatedSettingValue)
                    {
                        SendCameraSettingValue(Constants.MARKER_EXPOSURE_PACKET_STRING, value);                        
                    }
                }
            }
        }

        public double MarkerExposureSliderMin
        {
            get { return markerExposureSliderMin; }
            set
            {
                if (markerExposureSliderMin != value)
                    SetProperty(ref markerExposureSliderMin, value);
            }
        }

        public double MarkerExposureSliderMax
        {
            get { return markerExposureSliderMax; }
            set
            {
                if (markerExposureSliderMax != value)
                    SetProperty(ref markerExposureSliderMax, value);
            }
        }

        public double MarkerThresholdSliderValue
        {
            get { return markerThresholdSliderValue; }
            set
            {
                if (markerThresholdSliderValue != value)
                {
                    SetProperty(ref markerThresholdSliderValue, value);

                    // Set label value                    
                    MarkerThresholdValueLabel = Convert.ToInt32(value).ToString(); 

                    // Send it to QTM if value was set locally
                    if (!qtmUpdatedSettingValue)
                    {
                        SendCameraSettingValue(Constants.MARKER_THRESHOLD_PACKET_STRING, value);
                    }
                }
            }
        }
            
        public double MarkerThresholdSliderMin
        {
            get { return markerThresholdSliderMin; }
            set
            {
                if (markerThresholdSliderMin != value)
                    SetProperty(ref markerThresholdSliderMin, value);
            }
        }

        public double MarkerThresholdSliderMax
        {
            get { return markerThresholdSliderMax; }
            set
            {
                if (markerThresholdSliderMax != value)
                    SetProperty(ref markerThresholdSliderMax, value);
            }
        }

        // Video Drawer
        public double VideoExposureSliderValue
        {
            get { return videoExposureSliderValue; }
            set
            {
                if (videoExposureSliderValue != value)
                {
                    SetProperty(ref videoExposureSliderValue, value);

                    // Set label value                    
                    VideoExposureValueLabel = Convert.ToInt32(value).ToString();

                    // Send it to QTM if value was set locally
                    if (!qtmUpdatedSettingValue)
                    {
                        SendCameraSettingValue(Constants.VIDEO_EXPOSURE_PACKET_STRING, value);
                    }   

                    // Flash time cannot be greater than video exposure
                    if (videoExposureSliderValue < videoFlashSliderValue)
                        VideoFlashSliderValue = videoExposureSliderValue;
                }
            }
        }

        public double VideoExposureSliderMin
        {
            get { return videoExposureSliderMin; }
            set
            {
                if (videoExposureSliderMin != value)
                    SetProperty(ref videoExposureSliderMin, value);
            }
        }

        public double VideoExposureSliderMax
        {
            get { return videoExposureSliderMax; }
            set
            {
                if (videoExposureSliderMax != value)
                    SetProperty(ref videoExposureSliderMax, value);
            }
        }

        public double VideoFlashSliderValue
        {
            get { return videoFlashSliderValue; }
            set
            {
                // Flash time cannot be greater than video exposure
                if(value > videoExposureSliderValue)
                {
                    value = videoExposureSliderValue;
                }

                if (videoFlashSliderValue != value)
                {
                    SetProperty(ref videoFlashSliderValue, value);
                    
                    // Set label value                    
                    VideoFlashValueLabel = Convert.ToInt32(value).ToString();

                    // Send it to QTM if value was set locally
                    if (!qtmUpdatedSettingValue)
                    {
                        SendCameraSettingValue(Constants.VIDEO_FLASH_PACKET_STRING, value);
                    }
                }
            }
        }

        public double VideoFlashSliderMin
        {
            get { return videoFlashSliderMin; }
            set
            {
                if (videoFlashSliderMin != value)
                    SetProperty(ref videoFlashSliderMin, value);
            }
        }

        public double VideoFlashSliderMax
        {
            get { return videoFlashSliderMax; }
            set
            {
                if (videoFlashSliderMax != value)
                    SetProperty(ref videoFlashSliderMax, value);
            }
        }

        public void SetModelReference(CameraPage _ref)
        {
            // Set reference to model
            cameraPageModel = _ref;

            // Copy drawer references
            markerDrawerFrame = cameraPageModel.GetMarkerDrawerFrame();
            videoDrawerFrame = cameraPageModel.GetVideoDrawerFrame();

            // Switch them drawers now
            SwitchDrawers(CameraStore.CurrentCamera.Mode);
        }

        private void SwitchDrawers(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:

                    videoDrawerFrame.IsVisible = false;
                    markerDrawerFrame.IsVisible = true;
                                        
                    break;

                case CameraMode.ModeVideo:

                    markerDrawerFrame.IsVisible = false;
                    videoDrawerFrame.IsVisible = true;

                    break;
            }
        }

        /// <summary>
        /// Hides all drawers
        /// </summary>
        private void HideDrawer()
        {
            videoDrawerFrame.IsVisible = false;
            markerDrawerFrame.IsVisible = false;
        }

        /// <summary>
        /// Shows current drawer
        /// </summary>
        private void ShowDrawer()
        {
            SwitchDrawers(CameraStore.CurrentCamera.Mode);
        }

        string markerExposureValueString, markerThresholdValueString,
               videoExposureValueString, videoFlashValueString;

        public string MarkerExposureValueLabel
        {
            get { return markerExposureValueString; }
            set
            {
                if (markerExposureValueString != value)
                    SetProperty(ref markerExposureValueString, value);
            }
        }

        public string MarkerThresholdValueLabel
        {
            get { return markerThresholdValueString; }
            set
            {
                if (markerThresholdValueString != value)
                    SetProperty(ref markerThresholdValueString, value);
            }
        }

        public string VideoExposureValueLabel
        {
            get { return videoExposureValueString; }
            set
            {
                if (videoExposureValueString != value)
                    SetProperty(ref videoExposureValueString, value);
            }
        }

        public string VideoFlashValueLabel
        {
            get { return videoFlashValueString; }
            set
            {
                if (videoFlashValueString != value)
                    SetProperty(ref videoFlashValueString, value);
            }
        }
    }
}
