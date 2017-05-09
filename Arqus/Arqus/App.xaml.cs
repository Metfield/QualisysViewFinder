using Arqus.Services;
using Prism.Unity;
using Xamarin.Forms;
using Arqus.Helpers;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using Arqus.Services.MobileCenterService;
using Arqus.Service;

namespace Arqus
{
    public partial class  App : PrismApplication
    {
        CameraStreamService cameraService;

        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();
            

            MessagingService.Subscribe<Application>(this, MessageSubject.CONNECTED, (sender) =>
            {
                OnConnected();
            });

            MessagingService.Subscribe<Application>(this, MessageSubject.DISCONNECTED, (sender) =>
            {
                OnDisconnected();
            });

            NavigationService.NavigateAsync("NavigationPage/ConnectionPage");
        }

        protected override void RegisterTypes()
        {
            // Register types for navigation
            Container.RegisterTypeForNavigation<NavigationPage>();
            Container.RegisterTypeForNavigation<ConnectionPage>();
            Container.RegisterTypeForNavigation<CameraPage>();
            Container.RegisterTypeForNavigation<GridPage>();
        }

       
        protected override void OnStart()
        {
            base.OnStart();
            MobileCenterService.Init();
        }
        

        protected void OnConnected()
        {
        }

        protected void OnDisconnected()
        {
            SettingsService.Dispose();
        }
        
    }
}
