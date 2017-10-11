using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Arqus
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AboutPage : ContentPage
	{
        private double fontSize;
        public double FontSize
        {
            set
            {
                if (fontSize != value)
                {
                    fontSize = value;
                    OnPropertyChanged("FontSize");
                }
            }
            get { return fontSize; }
        }

		public AboutPage ()
		{
            InitializeComponent ();

            BindingContext = this;

            // Just use conditional compilation for this
            string buildversion = "Version " +
#if __ANDROID__
                Android.App.Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, 0).VersionName;
#elif __IOS__
                Foundation.NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];
#endif

            buildVersion.Text = buildversion;
            CalculateFontSize();
		}

        private void CalculateFontSize()
        {
#if __ANDROID__
            // Get standard size for Android
            FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label));
            return;
#endif
            // Do only for iOS
            FontSize = ((App)App.Current).MainPage.Height / 56.8;
        }
	}
}