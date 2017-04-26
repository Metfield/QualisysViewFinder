using Arqus.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    public class CameraPageViewModel : BindableBase, INavigationAware
    {
        private INavigationService navigationService;
        private CameraSettingsDrawer settingsDrawer;
        private List<CameraSettings> cameraSettings;

        // Hold slider min and max values
        private const float SLIDER_MIN = 0.0f,
                            SLIDER_MAX = 1.0f;

        // Where FS: First slider & SS: Second slider
        // Change these when changing mode
        private float CURRENT_CAM_FS_MIN,
                      CURRENT_CAM_FS_MAX,
                      CURRENT_CAM_SS_MIN,
                      CURRENT_CAM_SS_MAX;

        // This will hold the current, temporary camera settings
        List<QTMRealTimeSDK.Settings.SettingsGeneralCameraSystem> tempGeneralSettings;

        // Used to determine if whether Arqus should send new
        // camera setting value to QTM or not. We don't want to
        // send it to QTM if we just got it from it...
        bool qtmUpdatedSettingValue = false;

        public DelegateCommand GetStreamDataCommand { get; set; }
        public DelegateCommand SetCameraModeToMarkerCommand { get; set; }
        public DelegateCommand SetCameraModeToVideoCommand { get; set; }
        public DelegateCommand SetCameraModeToIntensityCommand { get; set; }
        public DelegateCommand OnAppearingCommand { get; set; }

        private CameraMode currentMode;

        public CameraMode CurrentMode
        {
            get { return currentMode; }
            set { SetProperty(ref currentMode, value); }
        }

        public CameraPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));

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
                
            // Create camera settings array
            cameraSettings = new List<CameraSettings>();
            // Get latest camera settings
            tempGeneralSettings = SettingsService.GetCameraSettings();

            // Create each camera settings object with a camera id
            for (int i = 1; i <= SettingsService.GetCameraCount(); i++)
            {
                CameraSettings camSettings = new CameraSettings(i);

                camSettings.MarkerExposure = tempGeneralSettings[i - 1].MarkerExposure.Current;
                camSettings.MarkerThreshold = tempGeneralSettings[i - 1].MarkerThreshold.Current;
                camSettings.VideoExposure = tempGeneralSettings[i - 1].VideoExposure.Current;
                camSettings.VideoFlash = tempGeneralSettings[i - 1].VideoFlashTime.Current;

                cameraSettings.Add(camSettings);
            }

            // Create Camera Settings Drawer object
            settingsDrawer = new CameraSettingsDrawer(this, CameraStore.CurrentCamera.ID,
                                                      tempGeneralSettings[CameraStore.CurrentCamera.ID - 1],
                                                      SLIDER_MIN, SLIDER_MAX);

            // Change the drawer mode
            settingsDrawer.SetDrawerMode(CameraStore.CurrentCamera.Mode, cameraSettings[CameraStore.CurrentCamera.ID - 1]);

            // Set the current mode
            SetCameraMode(CameraStore.CurrentCamera.Mode);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            try
            {
                NavigationMode navigationMode = (NavigationMode)parameters["__NavigationMode"];

                if (navigationMode == NavigationMode.Back)
                    MessagingCenter.Send(Application.Current, MessageSubject.DISCONNECTED.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {

        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            MessagingCenter.Send(Application.Current, MessageSubject.CONNECTED.ToString());
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            CameraStore.SetCurrentCamera(cameraID);
        }

        private void SetCameraMode(CameraMode mode)
        {
            // Change camera state mode
            if (CameraStore.CurrentCamera.Mode == mode)
                return;

            CameraStore.CurrentCamera.Mode = mode;

            // Change the drawer layout
            //settingsDrawer.SetDrawerMode(mode, cameraSettings[CameraStore.CurrentCamera.ID - 1]);

            // Change min and max values to convert to according 
            // to camera mode
            //SetCameraRangeConvertValues(mode);

            // Set the mode
           Task.Run(() => SetCameraMode());
        }

        private async void SetCameraMode()
        {
            await SettingsService.SetCameraMode(CameraStore.CurrentCamera.ID, CameraStore.CurrentCamera.Mode);
            MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + CameraStore.CurrentCamera.ID, CameraStore.CurrentCamera.Mode);
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
            // NOTE: As of this time this is highly unstable!
            // TODO: Uh.. something about it?
            tempGeneralSettings = SettingsService.GetCameraSettings();

            // Safe guard for shit crash which is not really working
            if (tempGeneralSettings == null)
                return;

            // Update camera settings values, then convert it and set it in slider
            if (CameraStore.CurrentCamera.Mode != CameraMode.ModeVideo)
            {
                // Marker or Marker Intensity mode                
                cameraSettings[CameraStore.CurrentCamera.ID - 1].MarkerExposure = tempGeneralSettings[camIndex].MarkerExposure.Current;
                FirstSliderValue = DataOperations.ConvertRange(CURRENT_CAM_FS_MIN, CURRENT_CAM_FS_MAX,
                                                               SLIDER_MIN, SLIDER_MAX, tempGeneralSettings[camIndex].MarkerExposure.Current);

                cameraSettings[CameraStore.CurrentCamera.ID - 1].MarkerThreshold = tempGeneralSettings[camIndex].MarkerThreshold.Current;
                SecondSliderValue = DataOperations.ConvertRange(CURRENT_CAM_SS_MIN, CURRENT_CAM_SS_MAX,
                                                           SLIDER_MIN, SLIDER_MAX, tempGeneralSettings[camIndex].MarkerThreshold.Current);
            }
            else
            {
                // Video mode                
                cameraSettings[CameraStore.CurrentCamera.ID - 1].VideoExposure = tempGeneralSettings[camIndex].VideoExposure.Current;
                FirstSliderValue = DataOperations.ConvertRange(CURRENT_CAM_FS_MIN, CURRENT_CAM_FS_MAX,
                                                              SLIDER_MIN, SLIDER_MAX, tempGeneralSettings[camIndex].VideoExposure.Current);

                cameraSettings[CameraStore.CurrentCamera.ID - 1].VideoFlash = tempGeneralSettings[camIndex].VideoFlashTime.Current;
                SecondSliderValue = DataOperations.ConvertRange(CURRENT_CAM_SS_MIN, CURRENT_CAM_SS_MAX,
                                                           SLIDER_MIN, SLIDER_MAX, tempGeneralSettings[camIndex].VideoFlashTime.Current);
            }

            qtmUpdatedSettingValue = false;
        }

        /// <summary>
        /// All these values will temporarily hold sliders-related
        /// information
        /// 
        /// From this point everything concerns drawer menu and sliders.
        /// </summary>        
        private string firstSliderString,
                        secondSliderString;

        private float firstSliderValue,
                        firstSliderMinValue,
                        firstSliderMaxValue,
                        secondSliderValue,
                        secondSliderMinValue,
                        secondSliderMaxValue;

        // Command handlers for cammera settings
        public string FirstSliderString
        {
            get { return firstSliderString; }
            set { SetProperty(ref firstSliderString, value); }
        }

        public string SecondSliderString
        {
            get { return secondSliderString; }
            set { SetProperty(ref secondSliderString, value); }
        }

        public float FirstSliderValue
        {
            get { return firstSliderValue; }
            set
            {
                SetProperty(ref firstSliderValue, value);

                if (qtmUpdatedSettingValue)
                    return;

                if (CameraStore.CurrentCamera.Mode == CameraMode.ModeVideo)
                {
                    SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, Constants.VIDEO_EXPOSURE_PACKET_STRING, value);
                }
                else
                {
                    SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, Constants.MARKER_EXPOSURE_PACKET_STRING, value);
                }
            }
        }

        public float FirstSliderMinValue
        {
            get { return firstSliderMinValue; }
            set { SetProperty(ref firstSliderMinValue, value); }
        }

        public float FirstSliderMaxValue
        {
            get { return firstSliderMaxValue; }
            set { SetProperty(ref firstSliderMaxValue, value); }
        }

        public float SecondSliderValue
        {
            get { return secondSliderValue; }
            set
            {
                SetProperty(ref secondSliderValue, value);

                if (qtmUpdatedSettingValue)
                    return;

                if (CameraStore.CurrentCamera.Mode == CameraMode.ModeVideo)
                {
                    SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, Constants.VIDEO_FLASH_PACKET_STRING, value);
                }
                else
                {
                    SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, Constants.MARKER_THRESHOLD_PACKET_STRING, value);
                }
            }
        }

        public float SecondSliderMinValue
        {
            get { return secondSliderMinValue; }
            set { SetProperty(ref secondSliderMinValue, value); }
        }

        public float SecondSliderMaxValue
        {
            get { return secondSliderMaxValue; }
            set { SetProperty(ref secondSliderMaxValue, value); }
        }

        /// <summary>
        /// Sets current Min and Max values so that UpdateCameraSettings can
        /// convert from Camera range [x,y] to slider's range [0,1] 
        /// </summary>
        /// <param name="mode"></param>
        private void SetCameraRangeConvertValues(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:
                    // Get Marker Exposure range values
                    CURRENT_CAM_FS_MIN = settingsDrawer.GetMarkerExposureMin();
                    CURRENT_CAM_FS_MAX = settingsDrawer.GetMarkerExposureMax();

                    // Get Marker Threshold range values
                    CURRENT_CAM_SS_MIN = settingsDrawer.GetMarkerThresholdMin();
                    CURRENT_CAM_SS_MAX = settingsDrawer.GetMarkerThresholdMax();

                    break;
                case CameraMode.ModeVideo:
                    // Get Video Exposure range values
                    CURRENT_CAM_FS_MIN = settingsDrawer.GetVideoExposureMin();
                    CURRENT_CAM_FS_MAX = settingsDrawer.GetVideoExposureMax();

                    // Get Video flash range values
                    CURRENT_CAM_SS_MIN = settingsDrawer.GetVideoFlashMin();
                    CURRENT_CAM_SS_MAX = settingsDrawer.GetVideoFlashMax();
                    break;

                default:
                    // Handle exception
                    break;
            }
        }
    }
}
