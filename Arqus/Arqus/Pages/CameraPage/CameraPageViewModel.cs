﻿using Arqus.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{

    public class CameraPageViewModel : BindableBase, INavigationAware
    {
        private ISettingsService settingsService;
        private INavigationService navigationService;
        
        private CameraState cameraState;

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

            cameraState = new CameraState(1, CameraMode.ModeMarker);
            
            // Default to marker mode
            SetCameraMode();
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
            cameraState.Mode = mode;
            SetCameraMode();
        }

        private async void SetCameraMode()
        {
            await settingsService.SetCameraMode(cameraState.ID, cameraState.Mode);
            MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + cameraState.ID, cameraState);
        }


        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            //MessagingCenter.Send(Application.Current, MessageSubject.DISCONNECTED.ToString());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            //MessagingCenter.Send(Application.Current, MessageSubject.CONNECTED.ToString());
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            
        }
    }
}
