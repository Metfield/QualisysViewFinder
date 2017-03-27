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
        MarkerApplication currentApplication;

        public MarkerPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartUrhoApp();
        }

        protected override void OnDisappearing()
        {
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            Debug.WriteLine("Back button pressed");
            return true;
        }

        async void StartUrhoApp()
        {
            // Start surface "sub-app" Tracking2DView
            currentApplication = await urhoSurface.Show<MarkerApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }

    }
}
