using Arqus.Helpers;
using Arqus.Service;
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

namespace Arqus
{
    public class CameraPageViewModel : BindableBase, INavigationAware
    {
        // Dependency services
        private INavigationService navigationService;

        // Keep track if latest value was updated by QTM
        public bool qtmUpdatedSettingValue = true;
        
        public CameraPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));
 
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
                }
                else
                {
                    cameraScreenLayout = "grid";
                    IsGridLayoutActive = true;
                }

                MessagingService.Send(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT, payload: new { cameraScreenLayout });
            });

            // We're starting with carousel mode
            isGridLayoutActive = false;

            MessagingCenter.Subscribe<CameraApplication, int>(this,
                MessageSubject.SET_CAMERA_SELECTION.ToString(),
                OnCameraSelection);
            
            MessagingCenter.Subscribe<QTMEventListener>(this,
                MessageSubject.CAMERA_SETTINGS_CHANGED.ToString(),
                (QTMEventListener sender) => { CameraSettings = CameraStore.CurrentCamera.GetSettings(); qtmUpdatedSettingValue = true; });
                

            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), CameraStore.CurrentCamera.ID);
            CameraSettings = CameraStore.CurrentCamera.GetSettings();

            // Reset flag
            qtmUpdatedSettingValue = false;

            // Switch them drawers now
            SwitchDrawers(CameraStore.CurrentCamera.Mode);
        }

        
        public void UpdateSetting(string setting, double value)
        {
            if (!qtmUpdatedSettingValue)
                SendCameraSettingValue(setting, value);

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
            MessagingService.Send(Application.Current, MessageSubject.CONNECTED, payload: new { Navigate = "OnNavigatingTo" });
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            // Set current camera
            CameraStore.SetCurrentCamera(cameraID);
            CameraSettings = CameraStore.CurrentCamera.GetSettings();
            
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
        
        private void SendCameraSettingValue(string setting, double value)
        {
            // Run this on separate thread to keep UI responsive
            Task.Run(() => SettingsService.SetCameraSettings(CameraStore.CurrentCamera.ID, setting, (float)value));
        }

        public DelegateCommand SetCameraModeToMarkerCommand { get; set; }
        public DelegateCommand SetCameraModeToVideoCommand { get; set; }
        public DelegateCommand SetCameraModeToIntensityCommand { get; set; }
        public DelegateCommand SetCameraScreenLayoutCommand { get; set; }

        private SettingsGeneralCameraSystem cameraSettings;

        public SettingsGeneralCameraSystem CameraSettings
        {
            get { return cameraSettings; }
            set { SetProperty(ref cameraSettings, value); }
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


        /// <summary>
        /// Shows current drawer
        /// </summary>
        private void ShowDrawer()
        {
            SwitchDrawers(CameraStore.CurrentCamera.Mode);
        }

    }
}
