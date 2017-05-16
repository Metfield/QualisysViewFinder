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
