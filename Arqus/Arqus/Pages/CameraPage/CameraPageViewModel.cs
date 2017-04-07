using Arqus.Helpers;
using Arqus.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
	public class CameraPageViewModel : BindableBase, INavigatedAware
	{
        private INavigationService navigationService;
        private ISettingsService service;

        private MarkerStream markerStream;
        private IntensityStream intensityStream;

        public CameraPageViewModel(INavigationService navigationService, ISettingsService service)
        {
            this.navigationService = navigationService;
            this.service = service;

            intensityStream = new IntensityStream();
            markerStream = new MarkerStream();

            SetCameraModeToMarkerCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarker));
            SetCameraModeToVideoCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeVideo));
            SetCameraModeToIntensityCommand = new DelegateCommand(() => SetCameraMode(CameraMode.ModeMarkerIntensity));

            // Set up messaging system for stream handling.
            // This is not optimal since it demands a lot
            // of messaging do render an image sequence
            // but for now it is to my knowledge the more
            // portable way of doing it.

            MessagingCenter.Subscribe<CameraApplication>(this, 
                MessageSubject.FETCH_IMAGE_DATA.ToString(), 
                OnFetchImageData);

            MessagingCenter.Subscribe<CameraApplication>(this,
                MessageSubject.FETCH_MARKER_DATA.ToString(),
                OnFetchMarkerData);

            // Default to marker mode
            SetCameraMode(CameraMode.ModeMarker);
        }

        

        private async void OnFetchImageData(Object sender)
        {
            List<ImageSharp.Color[]> imageData = await Task.Run(() => intensityStream.GetImageData());

            if(imageData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), imageData);
            }
        }

        private async void OnFetchMarkerData(Object sender)
        {
            List<QTMRealTimeSDK.Data.Camera> markerData = await Task.Run(() => markerStream.GetMarkerData());

            if (markerData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), markerData);
            }
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

        private async void SetCameraMode(CameraMode mode)
        {
            bool success = await service.SetCameraMode(20400, mode);

            if (success)
            {
                CurrentMode = mode;
                switch (mode)
                {
                    case CameraMode.ModeMarkerIntensity:
                        intensityStream.StartStream();
                        break;
                    case CameraMode.ModeMarker:
                        markerStream.StartStream();
                        break;
                    default:
                        throw new Exception("Stream mode no yet implemented");
                }


                MessagingCenter.Send(this, MessageSubject.STREAM_MODE_CHANGED.ToString(), mode);
            }
        }
    }
}
