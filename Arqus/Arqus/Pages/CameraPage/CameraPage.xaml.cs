using Arqus.Services;

using System;

using System.Diagnostics;

using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using System.Reactive.Linq;
using System.Threading;
using Arqus.Helpers;
using Arqus.Service;

namespace Arqus
{
    public partial class CameraPage : ContentPage
    {
        CameraApplication markerApplication;
        CameraPageViewModel viewModel;

        int throttleTime = 50;

        public CameraPage()
        {
            InitializeComponent();
            
            // The view model needs to be set initializing any observers that subscribes
            // to methods in the view model.
            viewModel = BindingContext as CameraPageViewModel;

            // Initialize slider observers
            InitSliderObservers();
        }
        
        // Moved into a function to keep the constructor from getting bloated with code
        private void InitSliderObservers()
        {
            // Marker-specific value bindings
            Observable.FromEventPattern<ValueChangedEventArgs>(markerExposureSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.MARKER_EXPOSURE_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(markerThresholdSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.MARKER_THRESHOLD_PACKET_STRING, value));

            // Video-specific value bindings
            Observable.FromEventPattern<ValueChangedEventArgs>(videoExposureSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.VIDEO_EXPOSURE_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(videoFlashTimeSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.VIDEO_FLASH_PACKET_STRING, value));

            // Extra video-specific value bindings for Lens control
            Observable.FromEventPattern<ValueChangedEventArgs>(lensFocusSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.LENS_FOCUS_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(lensApertureSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(throttleTime))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SnapAperture(value));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            markerApplication = await StartUrhoApp();

            // Notify ViewModel that loading for 3D app is done
            MessagingCenter.Send(this, MessageSubject.URHO_SURFACE_FINISHED_LOADING);
        }

        protected override async void OnDisappearing()
        {
            await markerApplication.Exit();
            UrhoSurface.OnDestroy();
            markerApplication = null;            

            viewModel.Dispose();
            viewModel = null;

            base.OnDisappearing();
        }       

        async Task<CameraApplication> StartUrhoApp()
        {
            markerApplication = await urhoSurface.Show<CameraApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
            return markerApplication;
        }        
    }
}

