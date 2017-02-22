using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    class Tracking3DPage : ContentPage
    {
        UrhoSurface urhoSurface;
        Tracking3DView surfaceApp;

        public Tracking3DPage()
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
            // Start surface "sub-app" Tracking3DView
            surfaceApp = await urhoSurface.Show<Tracking3DView>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }
}
