﻿using Arqus.Helpers;
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
        private INavigationService navigationService;

        private CameraState cameraState;
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

        private CameraMode currentMode;

        public CameraMode CurrentMode
        {
            get { return currentMode; }
            set { SetProperty(ref currentMode, value); }
        }

        Frame videoDrawerFrame, markerDrawerFrame;

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

            cameraState = CameraStore.State;
            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CameraStore.State.ID);            

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
            settingsDrawer = new CameraSettingsDrawer(this, CameraStore.State, cameraSettings[CameraStore.State.ID - 1]);

            // Reset flag
            qtmUpdatedSettingValue = false;
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
            cameraState.ID = cameraID;
            UpdateCameraSettings(this);
        }
        
        private void SetCameraMode(CameraMode mode)
        {           
            // Change camera state mode
            cameraState.Mode = mode;

            // Set the mode
            SetCameraMode();

            // Switch drawer scheme
            SwitchDrawers(cameraState.Mode);
        }

        private async void SetCameraMode()
        {
            await SettingsService.SetCameraMode(cameraState.ID, cameraState.Mode);
            MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + cameraState.ID, cameraState.Mode);
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
            int camIndex = cameraState.ID - 1;

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


        /// <summary>
        /// Will be called from CameraPage.xaml.cs 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /*public async void OnFirstSliderValueChangedFromUI(object sender, ValueChangedEventArgs args)
        {
            // Only update value if it was set by UI, not by QTM
            if (qtmUpdatedSettingValue || (args.NewValue > 1))
            {
                qtmUpdatedSettingValue = false;
                return;
            }

            // Set UI update flag
            uiUpdatedSettingValue = true;

            // Check for stream mode
            // Send the value to QTM and then update local structure
            if (cameraState.Mode == CameraMode.ModeVideo)
            {
                cameraSettings[cameraState.ID - 1].VideoExposure = (float)args.NewValue;
                await SettingsService.SetCameraSettings(cameraState.ID, Constants.VIDEO_EXPOSURE_PACKET_STRING, (float)args.NewValue);                
            }
            else
            {
                cameraSettings[cameraState.ID - 1].MarkerExposure = (float)args.NewValue;
                await SettingsService.SetCameraSettings(cameraState.ID, Constants.MARKER_EXPOSURE_PACKET_STRING, (float)args.NewValue);                
            }
        }*/

        /// <summary>
        /// Will be called from CameraPage.xaml.cs 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
       /* public async void OnSecondSliderValueChangedFromUI(object sender, ValueChangedEventArgs args)
        {            
            // Only update value if it was set by UI, not by QTM
            if (qtmUpdatedSettingValue || (args.NewValue > 1))
            {
                qtmUpdatedSettingValue = false;
                return;
            }

            // Set UI update flag
            uiUpdatedSettingValue = true;

            // Check for stream mode
            // Send the value to QTM and then update local structure
            if (cameraState.Mode == CameraMode.ModeVideo)
            {
                cameraSettings[cameraState.ID - 1].VideoFlash = (float)args.NewValue;
                await SettingsService.SetCameraSettings(cameraState.ID, Constants.VIDEO_FLASH_PACKET_STRING, (float)args.NewValue);                
            }
            else
            {
                cameraSettings[cameraState.ID - 1].MarkerThreshold = (float)args.NewValue;
                await SettingsService.SetCameraSettings(cameraState.ID, Constants.MARKER_THRESHOLD_PACKET_STRING, (float)args.NewValue);                
            }
        }*/

        private async void SendCameraSettingValue(string setting, double value)
        {
            await SettingsService.SetCameraSettings(CameraStore.State.ID, setting, (float)value);
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

                    // Send it to QTM if value was set locally
                    if (!qtmUpdatedSettingValue)
                    {
                        SendCameraSettingValue(Constants.VIDEO_EXPOSURE_PACKET_STRING, value);
                    }
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
                if (videoFlashSliderValue != value)
                {
                    SetProperty(ref videoFlashSliderValue, value);

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
            SwitchDrawers(cameraState.Mode);
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
    }
}
