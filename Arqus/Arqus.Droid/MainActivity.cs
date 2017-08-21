using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Android.Content.Res;

namespace Arqus.Droid
{
    [Activity(Label = "MainActivity", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, MainLauncher = false)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
            // set the layout resources first
            global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;
            global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity.TabLayoutResource = Resource.Layout.tabs;

            base.OnCreate (bundle);

            UserDialogs.Init(() => (Activity)global::Xamarin.Forms.Forms.Context);
			global::Xamarin.Forms.Forms.Init (this, bundle);

            LoadApplication (new Arqus.App ());
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }

        // Called when sent to background
        protected override void OnPause()
        {
            // This flag is used by CameraPage.xaml.cs::OnDisappear()
            NativeSharedBridge.applicationIsEnteringBackground = true;
            base.OnPause();
        }

        // Called when first executed and when coming back from background
        protected override void OnResume()
        {
            NativeSharedBridge.applicationIsEnteringBackground = false;
            base.OnResume();
        }
    }
}