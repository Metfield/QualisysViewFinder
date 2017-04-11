using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Arqus
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GridPage : ContentPage
	{
        GridApplication gridScene;

		public GridPage()
		{
			InitializeComponent ();

            // Start 2D camera streaming
            CameraStream.Instance.StartStream(70, QTMRealTimeSDK.Data.ComponentType.Component2d);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            CreateUrhoSurface();
        }

        // Creates urho scene and assigns it to surface
        private async void CreateUrhoSurface()
        {
            // Create and initialize urhoSharp scene
            gridScene = await urhoSurface.Show<GridApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }
}
