using Acr.UserDialogs;
using Arqus.DataModels;
using Arqus.Helpers;
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
        public uint lastQTMUpdate = 0;

        // Keep tabs on demo mode
        private bool isDemoModeActive;

        // Used for snapping slider to pre-determined values
        private LensApertureSnapper lensApertureSnapper;

        // Used to know which video drawer is to be displayed
        // Gets modified by the segmented controls in the view
        private bool isLensControlActive;

        // Holds current title for camera page
        private string pageTitle;

        // Used to dismiss loading screen
        private IUserDialogs userDialogs;

        private CameraMode cameraMode;

        private CameraApplication urhoApplicationReference;

        // Used to avoid sending redundant settings data to QTM after selecting
        // a camera in carousel layout. 
        private bool ignoreSettingsAfterCameraSelection;

        public CameraPageViewModel(INavigationService navigationService, IUserDialogs userDialogs)
        {
            this.navigationService = navigationService;
            this.userDialogs = userDialogs;

            // Start with a visible drawer
            // Otherwise iOS will mess everything up..
            IsBottomSheetVisible = true;
            IsModeToolbarActive = true;
            IsDrawerActive = true;

            CurrentCamera = CameraManager.CurrentCamera;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));

            ToggleBottomSheetCommand = new DelegateCommand(() => {
                IsBottomSheetVisible = !isBottomSheetVisible;
            });

            SetCameraScreenLayoutCommand = new DelegateCommand(() =>
            {
                IsGridLayoutActive = true;
                MessagingCenter.Send(this, Messages.Subject.SET_CAMERA_SCREEN_LAYOUT, ScreenLayoutType.Grid);

                // Update the page title 
                UpdatePageTitle(true);
            });

            // We're starting with carousel mode
            IsGridLayoutActive = false;

            MessagingCenter.Subscribe<CarouselScreenLayout, int>(this,
                Messages.Subject.SET_CAMERA_SELECTION.ToString(),
                OnCameraSelection);

            MessagingCenter.Subscribe(this,
                Messages.Subject.CAMERA_SETTINGS_CHANGED.ToString(),
                (QTMEventListener sender) =>
                {
                    // TODO: Remove Urho dependency
                    lastQTMUpdate = Urho.Time.SystemTime;

                    CameraManager.RefreshSettings();
                    CurrentCamera = CameraManager.CurrentCamera;

                    if (cameraMode != CurrentCamera.Settings.Mode)
                        SetCameraMode(CurrentCamera.Settings.Mode);
                });

            // Used to hide drawer when the urho surface is tapped
            MessagingCenter.Subscribe<CameraApplication>(this, Messages.Subject.URHO_SURFACE_TAPPED, (sender) => 
            {
                if (isBottomSheetVisible)
                    IsBottomSheetVisible = false;
            });

            // Handle back-button presses on Android
            MessagingCenter.Subscribe<CameraApplication>(this, Messages.Subject.URHO_ANDROID_BACK_BUTTON_PRESSED, (sender) =>
            {
                // Go back to main menu if settings drawer is not visible
                if (isBottomSheetVisible)
                    IsBottomSheetVisible = false;
                else
                    Device.BeginInvokeOnMainThread(() => navigationService.GoBackAsync());
            });

            // Notifies of camera selection
            MessagingCenter.Send(this, Messages.Subject.SET_CAMERA_SELECTION.ToString(), CurrentCamera.ID);

            // Initialize lens aperture snapper
            lensApertureSnapper = new LensApertureSnapper(this);
            ApertureSnapMax = lensApertureSnapper.LookupTSize - 1;

            // Switch them drawers now
            SwitchDrawers(CurrentCamera.Settings.Mode);

            // No Lens control UI when we start (if even available)
            IsLensControlActive = false;

            UpdatePageTitle(IsGridLayoutActive);
        }

        public void SetCameraSetting(string setting, double value, bool bypassSkipCounter = false)
        {
            // If nothing has been recieved from QTM then update the settings
            // else wait 200 ms before updating qtm again to Xamarin.Forms time to update
            // and prevent an infinite feedback loop
            if (Urho.Time.SystemTime - lastQTMUpdate > 200 && !ignoreSettingsAfterCameraSelection)
                CurrentCamera.SetSetting(setting, value);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            // Not used
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            // Not used
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            // Related to iOS issue where this flag is set at the constructor
            // Hide the drawer now that Xamarin.Forms has finished loading the interface
            IsBottomSheetVisible = false;
            IsModeToolbarActive = false;
            IsDrawerActive = false;

            try
            {
                // Need to know if this is demo mode in order to start stream accordingly
                IsDemoModeActive = parameters.GetValue<bool>(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            } 

            if (QTMNetworkConnection.IsMaster || IsDemoModeActive)
            {
                IsModeToolbarActive = true;
                IsDrawerActive = true;
            }
            else
            {
                IsModeToolbarActive = false;
                IsDrawerActive = false;
            }

            // Dismiss loading screen once the page has finished loading
            if(!IsDemoModeActive)
                Task.Run(() => userDialogs.HideLoading());
        }

        /// <summary>
        /// Gets notified by CameraPage once the UrhoSurface has finished loading
        /// </summary>
        private void StartStreaming()
        {
            // Notify UrhoSurface Application of the stream mode start intent
            MessagingCenter.Send(this, Messages.Subject.STREAM_START, IsDemoModeActive);

            // Once this is done we unsubscribe to the msg
            MessagingCenter.Unsubscribe<CameraPage>(this, Messages.Subject.URHO_SURFACE_FINISHED_LOADING);
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            // Don't send camera settings to QTM 
            SetIgnoreSettingsAfterCameraSelection();

            // Set current camera
            CameraManager.SetCurrentCamera(cameraID);
            CurrentCamera = CameraManager.CurrentCamera;

            // Check if camera selection was done through grid mode
            if (IsGridLayoutActive)
            {
                IsGridLayoutActive = false;

                // Don't display drawer nor toolbar if this is slave mode
                if (!QTMNetworkConnection.IsMaster && !IsDemoModeActive)
                {
                    IsDrawerActive = false;
                    IsModeToolbarActive = false;
                }
            }

            // Switch drawer mode
            Device.BeginInvokeOnMainThread(() => SwitchDrawers(CurrentCamera.Settings.Mode));

            // Change camera page title
            UpdatePageTitle(IsGridLayoutActive);
        }

        // Set flag to avoid sending redundant data to QTM
        private async void SetIgnoreSettingsAfterCameraSelection()
        {
            Task.Run(() =>
            {
                ignoreSettingsAfterCameraSelection = true;
                    Thread.Sleep(500);
                ignoreSettingsAfterCameraSelection = false;
            });
        }

        private void SetCameraMode(CameraMode mode)
        {
            cameraMode = mode;
            CurrentCamera.SetMode(mode);

            // Switch drawer scheme
            if (QTMNetworkConnection.IsMaster)
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
        public DelegateCommand ToggleBottomSheetCommand { get; set; }

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

        private bool isModeToolbarActive = true;

        public bool IsModeToolbarActive
        {
            get { return isModeToolbarActive; }
            set { SetProperty(ref isModeToolbarActive, value); }
        }

        private bool isDrawerActive;

        public bool IsDrawerActive
        {
            get { return isDrawerActive; }
            set { SetProperty(ref isDrawerActive, value); }
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
                if (SetProperty(ref isGridLayoutActive, value))
                {
                    IsModeToolbarActive = !isGridLayoutActive;
                    IsDrawerActive = !isGridLayoutActive;
                }
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
            set
            {
                if (bottomSheet != null)
                {
                    // Attempt to access the bottom sheet only if it has been created
                    if (isBottomSheetVisible)
                    {
                        // Hide
                        Task.Run(async () =>
                        {
                            // Wait for animation to complete and then hide drawer
                            await bottomSheet.TranslateTo(bottomSheet.X, bottomSheetInitialPosition + bottomSheet.Height, 500, Easing.CubicOut);
                            SetProperty(ref isBottomSheetVisible, value);
                        });
                    }
                    else
                    {
                        // Show
                        SetProperty(ref isBottomSheetVisible, value);
                        bottomSheet.TranslateTo(bottomSheet.X, bottomSheetInitialPosition, 500, Easing.BounceOut);
                    }
                }
                else
                {
                    SetProperty(ref isBottomSheetVisible, value);
                }
            }
        }

        /// <summary>
        /// Shows current drawer
        /// </summary>
        private void ShowDrawer()
        {
            SwitchDrawers(CurrentCamera.Settings.Mode);
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
            SetCameraSetting(Constants.LENS_APERTURE_PACKET_STRING, SnappedValue, true);
        }

        public string PageTitle
        { 
            get { return pageTitle; }
            private set { SetProperty(ref pageTitle, value); }
        }
                
        private bool showGridIconOnToolbar;

        public bool ShowGridIconOnToolbar
        {
            get { return showGridIconOnToolbar; }
            private set { SetProperty(ref showGridIconOnToolbar, value); }
        }

        private void UpdatePageTitle(bool isGridLayout)
        {
            // Set page title
            if (isGridLayout)
            {
                PageTitle = Constants.TITLE_GRIDVIEW;
                ShowGridIconOnToolbar = false;
            }
            else
            {
                PageTitle = CurrentCamera.PageTitle + (IsDemoModeActive ? "" : (QTMNetworkConnection.IsMaster ? "" : " (slave)"));
                ShowGridIconOnToolbar = true;
            }
        }

        // Used for drawer animation
        private StackLayout bottomSheet;        
        private double bottomSheetInitialPosition;

        public void SetBottomSheetHandle(StackLayout handle)
        {
            bottomSheet = handle;
            bottomSheetInitialPosition = bottomSheet.Y;
        }

        public void SetUrhoApplicationReference(CameraApplication application)
        {
            // Copy reference
            urhoApplicationReference = application;

            // Application can now begin streaming
            StartStreaming();
        }

        public void Dispose()
        {
            navigationService = null;
            currentCamera = null;
            IsDemoModeActive = false;

            MessagingCenter.Unsubscribe<CameraApplication, int>(this, Messages.Subject.SET_CAMERA_SELECTION);
            MessagingCenter.Unsubscribe<QTMEventListener>(this, Messages.Subject.CAMERA_SETTINGS_CHANGED);
            MessagingCenter.Unsubscribe<CameraApplication>(this, Messages.Subject.URHO_SURFACE_TAPPED);
            MessagingCenter.Unsubscribe<CameraApplication>(this, Messages.Subject.URHO_ANDROID_BACK_BUTTON_PRESSED);

            GC.Collect();
        }
    }
}
