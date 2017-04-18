using Arqus.Helpers;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Arqus
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GridPage : ContentPage
	{
        GridApplication urhoScene;

		public GridPage()
		{
			InitializeComponent ();

            /*
            MessagingCenter.Subscribe<GridApplication, int>(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), (sender, cameraID) =>
            {
                var navigationParams = new NavigationParameters();
                navigationParams.Add("urho", urhoScene);
                //navigationParams.Add("cameraID", cameraID);

                UrhoSurface.OnPause();
                MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), navigationParams);
            });
            */
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            CreateUrhoSurface();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //UrhoSurface.OnDestroy();
        }

        // Creates urho scene and assigns it to surface
        private async void CreateUrhoSurface()
        {
            // Create and initialize urhoSharp scene
            urhoScene = await urhoSurface.Show<GridApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }
}
