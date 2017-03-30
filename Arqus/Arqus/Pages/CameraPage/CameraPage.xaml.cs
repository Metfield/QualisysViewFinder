using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    public partial class MarkerPage : ContentPage
    {
        CameraApplication currentApplication;

        public MarkerPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            currentApplication = await StartUrhoApp();
        }

        protected override void OnDisappearing()
        {
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }
        

        async Task<CameraApplication> StartUrhoApp()
        {
            // Start surface "sub-app" Tracking2DView
            CameraApplication markerApplication = await urhoSurface.Show<CameraApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
            return markerApplication;
        }

    }
}
