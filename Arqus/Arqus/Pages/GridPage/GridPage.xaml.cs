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
        private bool init = false;

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

            urhoScene = await CreateUrhoSurface();
            /*
            if (!init)
            {
                urhoScene = await CreateUrhoSurface();
                init = true;
            }
            else
                UrhoSurface.OnResume();
                */
        }

        protected override void OnDisappearing()
        {
            urhoScene.Exit();
            UrhoSurface.OnDestroy();
            //urhoSurface.BindingContext = null;
            base.OnDisappearing();
        }

        // Creates urho scene and assigns it to surface
        private async Task<GridApplication> CreateUrhoSurface()
        {
            // Create and initialize urhoSharp scene
            return await urhoSurface.Show<GridApplication>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }
    }
}
