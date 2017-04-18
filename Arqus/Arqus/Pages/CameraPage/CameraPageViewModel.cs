using Arqus.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Arqus
{
    public class CameraState
    {
        public uint ID { get; set; }
        public CameraMode Mode { get; set; }

        public CameraState(uint id, CameraMode mode)
        {
            ID = id;
            Mode = mode;
        }
    }

    public class CameraPageViewModel : BindableBase, INavigatedAware
	{
        private ISettingsService settingsService;
        private INavigationService navigationService;
        
        private CameraState cameraState;

        private bool isDetailed = false;

        public bool IsDetailed
        {
            get { return isDetailed; }
            set { SetProperty(ref isDetailed, value); }
        }


        public CameraPageViewModel(INavigationService navigationService, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
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

            cameraState = new CameraState(id: 1, mode: CameraMode.ModeMarker);
            cameraState.ID = 1;
            cameraState.Mode = CameraMode.ModeMarker;

            // Default to marker mode
            SetCameraMode();
        }

        private void OnCameraSelection(Object sender, int cameraID)
        {
            cameraState.ID = (uint)cameraID;
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
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
            cameraState.Mode = mode;
            SetCameraMode();
        }

        private async void SetCameraMode()
        {
            await settingsService.SetCameraMode(cameraState.ID, cameraState.Mode);
            MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + cameraState.ID, cameraState);
        }
    }
}
