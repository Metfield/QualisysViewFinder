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

        private CameraState cameraState;

        private CameraSettingsDrawer settingsDrawer;
        private List<CameraSettings> cameraSettings;

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

            cameraState = CameraStore.State;
            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CameraStore.State.ID);

            // Create camera settings array
            cameraSettings = new List<CameraSettings>();

            List<QTMRealTimeSDK.Settings.SettingsGeneralCameraSystem> tempGeneralSettings = SettingsService.GetCameraSettings();
                        
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
            settingsDrawer = new CameraSettingsDrawer(this, CameraMode.ModeMarker, CameraStore.State.ID, 
                                                      tempGeneralSettings[CameraStore.State.ID - 1], 
                                                      cameraSettings[CameraStore.State.ID - 1]);
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            cameraState.ID = cameraID;
        }

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

        private void SetCameraMode(CameraMode mode)
        {
            // Change camera state mode
            cameraState.Mode = mode;

            // Change the drawer layout
            settingsDrawer.ChangeDrawerMode(mode, cameraSettings[cameraState.ID - 1]);

            // Set the mode
            SetCameraMode();
        }

        private async void SetCameraMode()
        {
            await SettingsService.SetCameraMode(cameraState.ID, cameraState.Mode);
            MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + cameraState.ID, cameraState.Mode);
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
            // Begin streaming 
            MessagingCenter.Send(Application.Current, MessageSubject.CONNECTED.ToString());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }

       
        // Settings Drawer section
        private string firstSliderString,
                       secondSliderString;

        private int firstSliderValue,
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

        public int FirstSliderValue
        {
            get { return firstSliderValue; }
            set
            {
                SetProperty(ref firstSliderValue, value);

                if (cameraState.Mode == CameraMode.ModeVideo)
                {
                    cameraSettings[cameraState.ID - 1].VideoExposure = value;
                    SettingsService.SetCameraSettings(cameraState.ID, Constants.VIDEO_EXPOSURE_PACKET_STRING, value);
                }
                else
                { 
                    cameraSettings[cameraState.ID - 1].MarkerExposure = value;
                    SettingsService.SetCameraSettings(cameraState.ID, Constants.MARKER_EXPOSURE_PACKET_STRING, value);
                }
            }
        }

        public int FirstSliderMinValue
        {
            get { return firstSliderMinValue; }
            set { SetProperty(ref firstSliderMinValue, value); }
        }

        public int FirstSliderMaxValue
        {
            get { return firstSliderMaxValue; }
            set { SetProperty(ref firstSliderMaxValue, value); }
        }

        public int SecondSliderValue
        {
            get { return secondSliderValue; }
            set
            {
                SetProperty(ref secondSliderValue, value);

                if (cameraState.Mode == CameraMode.ModeVideo)
                {
                    cameraSettings[cameraState.ID - 1].VideoFlash = value;
                    SettingsService.SetCameraSettings(cameraState.ID, Constants.VIDEO_FLASH_PACKET_STRING, value);
                }
                else
                {
                    cameraSettings[cameraState.ID - 1].MarkerThreshold = value;
                    SettingsService.SetCameraSettings(cameraState.ID, Constants.MARKER_THRESHOLD_PACKET_STRING, value);
                }
            }
        }

        public int SecondSliderMinValue
        {
            get { return secondSliderMinValue; }
            set { SetProperty(ref secondSliderMinValue, value); }
        }

        public int SecondSliderMaxValue
        {
            get { return secondSliderMaxValue; }
            set { SetProperty(ref secondSliderMaxValue, value); }
        }
    }
}
