using Prism.Unity;
using Xamarin.Forms;

using Microsoft.Practices.Unity;
using Urho.Forms;

using Acr.UserDialogs;

namespace Arqus
{
    public partial class  App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();
            NavigationService.NavigateAsync("NavigationPage/ConnectionPage");
        }

        protected override void RegisterTypes()
        {
            // Register types for navigation
            Container.RegisterTypeForNavigation<NavigationPage>();
            Container.RegisterTypeForNavigation<ConnectionPage>();
            Container.RegisterTypeForNavigation<CameraPage>();
            Container.RegisterTypeForNavigation<AboutPage>();

            Container.RegisterInstance<IUserDialogs>(UserDialogs.Instance);
        }
       
        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            UrhoSurface.OnPause();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            UrhoSurface.OnResume();
            base.OnResume();
        }
    }
}
