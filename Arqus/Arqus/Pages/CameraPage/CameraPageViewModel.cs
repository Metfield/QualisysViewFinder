using Arqus.DataModels;
using Arqus.Helpers;
using Arqus.Service;
using Arqus.Services;
using Arqus.Services.MobileCenterService;
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
    public class CameraPageViewModel : BindableBase, INavigationAware
    {
        // Dependency services
        private INavigationService navigationService;
        
        // Keep track if latest value was updated by QTM
        public uint skipCounter = 0;

        
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

            });

            // We're starting with carousel mode
            isGridLayoutActive = false;

            MessagingCenter.Subscribe<CameraApplication, int>(this,
                MessageSubject.SET_CAMERA_SELECTION.ToString(),
                OnCameraSelection);
            
            MessagingCenter.Subscribe(this,
                MessageSubject.CAMERA_SETTINGS_CHANGED.ToString(),
                (QTMEventListener sender) =>
                {
                    skipCounter++;
                    // REMOVED FOR DEBUG PURPOSE
                    //CurrentCamera.UpdateSettings();
                });
            
            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CurrentCamera.ID);

            // Switch them drawers now
            SwitchDrawers(CurrentCamera.Mode);
        }

        public void SetCameraSetting(string setting, double value)
        {
            if(skipCounter == 0)
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

                // Invoke on main thread to avoid exception
                Device.BeginInvokeOnMainThread(() => SwitchDrawers(CurrentCamera.Mode));

                return;
            }

            // Switch drawer mode
            Device.BeginInvokeOnMainThread(() => SwitchDrawers(CurrentCamera.Mode));
        }

        private void SetCameraMode(CameraMode mode)
        {
            // Set the mode            
            CurrentCamera.SetMode(mode);

            // Switch drawer scheme
            SwitchDrawers(CurrentCamera.Mode);
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

    }
}
