using Arqus.Services;
using Prism.Unity;
using Xamarin.Forms;
using Arqus.Helpers;
using Microsoft.Practices.Unity;

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
            Container.RegisterTypeForNavigation<OnlineStreamMenuPage>();
            Container.RegisterTypeForNavigation<MarkerPage>();

            //Container.RegisterType<IImageProcessor, ImageProcessor>();
        }


    }
}
