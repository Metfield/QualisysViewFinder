using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    class Tracking2DPage : ContentPage
    {
        UrhoSurface urhoSurface;
        MarkerApplication surfaceApp;

        public Tracking2DPage()
        {
            // Create Urho Surface
            urhoSurface = new UrhoSurface();
            urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;

            // Create layout with urho surface
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    urhoSurface
                }
            };
        }

        protected override async void OnAppearing()
        {
            StartUrhoApp();
        }

        async void StartUrhoApp()
        {
            // Start surface "sub-app" Tracking2DView
            surfaceApp = await urhoSurface.Show<MarkerApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }
}
