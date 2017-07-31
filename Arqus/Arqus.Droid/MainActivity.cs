using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;

namespace Arqus.Droid
{
    [Activity(Label = "MainActivity", MainLauncher = false)]
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
	}
}