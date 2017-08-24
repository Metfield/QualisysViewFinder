using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Arqus
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();
            
            // Just use conditional compilation for this
            string buildversion = "Version " +
#if __ANDROID__
                Android.App.Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, 0).VersionName;
#elif __IOS__
                Foundation.NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];
#endif
            buildVersion.Text = buildversion;
		}
	}
}