using Arqus.Services;
using Prism.Unity;
using Xamarin.Forms;
using Arqus.Helpers;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using Arqus.Services.MobileCenterService;
using Arqus.Service;
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
            Container.RegisterTypeForNavigation<GridPage>();

            Container.RegisterInstance<IUserDialogs>(UserDialogs.Instance);
        }

       
        protected override void OnStart()
        {
            base.OnStart();
            MobileCenterService.Init();
        }
        
    }
}
