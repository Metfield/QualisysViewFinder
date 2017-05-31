using Arqus.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Prism.Mvvm;
using Arqus.Helpers;

namespace Arqus
{
    public partial class CameraPage : ContentPage
    {
        CameraApplication currentApplication;
        CameraPageViewModel viewModel;

        public CameraPage()
        {
            InitializeComponent();
            
            // The view model needs to be set initializing any observers that subscribes
            // to methods in the view model.
            viewModel = BindingContext as CameraPageViewModel;
            InitSliderObservers();
        }

        // Moved into a function to keep the constructor from getting bloated with code
        private void InitSliderObservers()
        {
            Observable.FromEventPattern<ValueChangedEventArgs>(markerExposureSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.MARKER_EXPOSURE_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(markerThresholdSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.MARKER_THRESHOLD_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(videoExposureSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.VIDEO_EXPOSURE_PACKET_STRING, value));

            Observable.FromEventPattern<ValueChangedEventArgs>(videoFlashTimeSlider, "ValueChanged")
                .Select(eventPattern => eventPattern.EventArgs.NewValue)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe((value) => viewModel.SetCameraSetting(Constants.VIDEO_FLASH_PACKET_STRING, value));

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            currentApplication = await StartUrhoApp();                      

            // Set model reference
            //((CameraPageViewModel)BindingContext).SetModelReference(this);
        }

        protected override async void OnDisappearing()
        {
            //await currentApplication.Exit();
            UrhoSurface.OnDestroy();
            await Task.Delay(250);
            base.OnDisappearing();
        }        

        async Task<CameraApplication> StartUrhoApp()
        {
            CameraApplication markerApplication = await urhoSurface.Show<CameraApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
            return markerApplication;
        }
        
    }
}
