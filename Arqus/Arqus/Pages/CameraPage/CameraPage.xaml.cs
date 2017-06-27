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
        CameraApplication application;
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

        DeviceOrientations orientation;

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

        /// <summary>
        /// Updates the Urho application in case there is an orientation change
        /// </summary>
        /// <param name="width">width of the screen</param>
        /// <param name="height">height of the screen</param>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // If the width is greated than the height the device is in landscape mode,
            // otherwise it is in portrait
            if(width > height)
            {
                orientation = DeviceOrientations.Landscape;
            }
            else
            {
                orientation = DeviceOrientations.Portrait;
            }

            // Only update the application if it has been created
            if (application != null)
                application.Orientation = orientation;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartUrhoApp();
        }

        protected override async void OnDisappearing()
        {
            await application.Exit();
            UrhoSurface.OnDestroy();
            application = null;            

            // NOTE: Check this when wokring demo mode is merged with this version
            //viewModel.Dispose();
            //viewModel = null;

            base.OnDisappearing();
        }        
        

        async void StartUrhoApp()
        {
            application = await urhoSurface.Show<CameraApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
            //Set the orientation of the application to match the rest of the UI
            application.Orientation = orientation;
            

            // Notify ViewModel that loading for 3D app is done
            MessagingCenter.Send(this, MessageSubject.URHO_SURFACE_FINISHED_LOADING);
        }
        
    }
}

