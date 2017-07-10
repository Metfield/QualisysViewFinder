﻿using Arqus.DataModels;
using Arqus.Helpers;
using Arqus.Service;
using Arqus.Services;
using Arqus.Services.MobileCenterService;
using Arqus.Visualization;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static Arqus.CameraApplication;

namespace Arqus
{
    public class CameraPageViewModel : BindableBase, INavigationAware, IDisposable
    {
        // Dependency services
        private INavigationService navigationService;
        
        // Keep track if latest value was updated by QTM
        public uint skipCounter = 0;
        
        // Keep tabs on demo mode
        private bool isDemoModeActive;

        // Used for snapping slider to pre-determined values
        private LensApertureSnapper lensApertureSnapper;

        // Used to know which video drawer is to be displayed
        // Gets modified by the segmented controls in the view
        private bool isLensControlActive;

        // Holds current title for camera page
        private string pageTitle;
        
        public CameraPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            CurrentCamera = CameraStore.CurrentCamera;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));

            HideBottomSheetCommand = new DelegateCommand(() => {
                IsBottomSheetVisible = !isBottomSheetVisible;
            });

            SetCameraScreenLayoutCommand = new DelegateCommand(() =>
            {
                string cameraScreenLayout;
                // Hide/show drawer according to mode
                // We don't want to show any drawers in grid mode
                if (isGridLayoutActive)
                {
                    cameraScreenLayout = "carousel";
                    IsGridLayoutActive = false;
                    ShowDrawer();

                    MessagingService.Send(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT, ScreenLayoutType.Carousel, payload: new { cameraScreenLayout });
                }
                else
                {
                    cameraScreenLayout = "grid";
                    IsGridLayoutActive = true;
                    MessagingService.Send(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT, ScreenLayoutType.Grid, payload: new { cameraScreenLayout });
                }

                // Update the page title 
                UpdatePageTitle();
            });

            // We're starting with carousel mode
            isGridLayoutActive = false;

            MessagingCenter.Subscribe<CarouselScreenLayout, int>(this,
                MessageSubject.SET_CAMERA_SELECTION.ToString(),
                OnCameraSelection);
            
            MessagingCenter.Subscribe(this,
                MessageSubject.CAMERA_SETTINGS_CHANGED.ToString(),
                (QTMEventListener sender) =>
                {
                    skipCounter++;
                    // REMOVED FOR DEBUG PURPOSE

                    CurrentCamera.UpdateSettings();
                    SetCameraMode(CurrentCamera.Settings.Mode);
                });

            MessagingCenter.Subscribe<CameraPage>(this,
                MessageSubject.URHO_SURFACE_FINISHED_LOADING, (sender) => StartStreaming());
                        
            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CurrentCamera.ID);

            // Initialize lens aperture snapper
            lensApertureSnapper = new LensApertureSnapper(this);
            ApertureSnapMax = lensApertureSnapper.LookupTSize - 1;

            // Switch them drawers now
            SwitchDrawers(CurrentCamera.Mode);

            // No Lens control UI when we start (if even available)
            IsLensControlActive = false;

            UpdatePageTitle();
        }

        public void SetCameraSetting(string setting, double value)
        {
            // If nothing has been recieved from QTM then update the settings
            // If one or several events has been recieved skip updating QTM an decrement the counter
            if (skipCounter == 0)
                CurrentCamera.SetSetting(setting, value);
            else
                skipCounter--;
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedFrom");
            
            try
            {
                NavigationMode navigationMode = parameters.GetValue<NavigationMode>("NavigationMode");

                //if (navigationMode == NavigationMode.Back)
                    //MessagingCenter.Send(Application.Current, MessageSubject.DISCONNECTED);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedTo");

            try
            {
                // Need to know if this is demo mode in order to start stream accordingly
                IsDemoModeActive = parameters.GetValue<bool>(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Gets notified by CameraPage once the UrhoSurface has finished loading
        /// </summary>
        private void StartStreaming()
        {
            // Notify UrhoSurface Application of the stream mode start intent
            MessagingService.Send(this, MessageSubject.STREAM_START, IsDemoModeActive);

            // Once this is done we unsubscribe to the msg
            MessagingCenter.Unsubscribe<CameraPage>(this, MessageSubject.URHO_SURFACE_FINISHED_LOADING);
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            MessagingService.Send(Application.Current, MessageSubject.CONNECTED, payload: new { Navigate = "OnNavigatingTo" });
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            // Set current camera
            CameraStore.SetCurrentCamera(cameraID);
            CurrentCamera = CameraStore.CurrentCamera;
            
            // Check if camera selection was done through grid mode
            if (IsGridLayoutActive)
            {
                IsGridLayoutActive = false;
            }

            // Switch drawer mode
            Device.BeginInvokeOnMainThread(() => SwitchDrawers(CurrentCamera.Mode));

            // Change camera page title
            UpdatePageTitle();
        }

        private void SetCameraMode(CameraMode mode)
        {
            // Set the mode            
            CurrentCamera.SetMode(mode);

            // Switch drawer scheme
            SwitchDrawers(mode);
        }
        
        private void SendCameraSettingValue(string setting, double value)
        {
            // Run this on separate thread to keep UI responsive
            Task.Run(() => CurrentCamera.SetSetting(setting, value));
        }

        public DelegateCommand SetCameraModeToMarkerCommand { get; set; }
        public DelegateCommand SetCameraModeToVideoCommand { get; set; }
        public DelegateCommand SetCameraModeToIntensityCommand { get; set; }
        public DelegateCommand SetCameraScreenLayoutCommand { get; set; }
        public DelegateCommand HideBottomSheetCommand { get; set; }

        private Camera currentCamera;
        public Camera CurrentCamera
        {
            get
            {
                return currentCamera;
            }
            set
            {
                SetProperty(ref currentCamera, value);
            }
        }

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

        private bool isMarkerMode;
        public bool IsMarkerMode
        {
            get
            {
                return isMarkerMode;
            }
            set
            {
                SetProperty(ref isMarkerMode, value);
            }
        }

        private bool isVideoMode;

        public bool IsVideoMode
        {
            get { return isVideoMode; }
            set { SetProperty(ref isVideoMode, value); }
        }

        private void SwitchDrawers(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:

                    IsVideoMode = false;
                    IsMarkerMode = true;

                    break;

                case CameraMode.ModeVideo:

                    IsMarkerMode = false;
                    IsVideoMode = true;

                    break;
            }
        }

        private bool isBottomSheetVisible = false;

        public bool IsBottomSheetVisible
        {
            get { return isBottomSheetVisible; }
            set { SetProperty(ref isBottomSheetVisible, value); }
        }

        /// <summary>
        /// Shows current drawer
        /// </summary>
        private void ShowDrawer()
        {
            SwitchDrawers(CurrentCamera.Mode);
        }

        // Convenient variable to handle a camera's max aperture        
        private double apertureSnapMax;
        public double ApertureSnapMax
        {
            get
            {
                return apertureSnapMax;
            }
            set
            {
                SetProperty(ref apertureSnapMax, value);
            }
        }

        // Used for displaying the value on the camera page's label
        private double snappedValue;
        public double SnappedValue
        {
            get { return snappedValue; }
            set { SetProperty(ref snappedValue, value); }
        }

        public bool IsLensControlActive
        {
            get { return isLensControlActive; }
            set { SetProperty(ref isLensControlActive, value); }
        }

        public bool IsDemoModeActive
        {
            get { return isDemoModeActive; }
            set { SetProperty(ref isDemoModeActive, value); }
        }

        // Handles Aperture value snapping and sets the setting
        public void SnapAperture(double value)
        {
            SnappedValue = lensApertureSnapper.OnSliderValueChanged(value);

            // Once value is snapped, set the aperture setting
            SetCameraSetting(Constants.LENS_APERTURE_PACKET_STRING, SnappedValue);
        }

        public string PageTitle
        { 
            get { return pageTitle; }
            private set { SetProperty(ref pageTitle, value); }
        }

        private void UpdatePageTitle()
        {
            // Set page title
            if (IsGridLayoutActive)
                PageTitle = Constants.TITLE_GRIDVIEW;
            else
                PageTitle = CurrentCamera.PageTitle;
        }

        public void Dispose()
        {
            navigationService = null;
            currentCamera = null;
            IsDemoModeActive = false;

            MessagingCenter.Unsubscribe<CameraApplication, int>(this, MessageSubject.SET_CAMERA_SELECTION);
            MessagingCenter.Unsubscribe<QTMEventListener>(this, MessageSubject.CAMERA_SETTINGS_CHANGED);            

            GC.Collect();
        }
    }
}
