using Arqus.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    public partial class CameraPage : ContentPage
    {
        CameraApplication currentApplication;


        public CameraPage()
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
            CameraApplication markerApplication = await urhoSurface.Show<CameraApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
            return markerApplication;
        }

    }
}
