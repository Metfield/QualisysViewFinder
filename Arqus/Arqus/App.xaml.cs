using Arqus.Services;
using Prism.Unity;
using Xamarin.Forms;
using Arqus.Helpers;
using Microsoft.Practices.Unity;
using System.Diagnostics;

namespace Arqus
{
    public partial class  App : PrismApplication
    {
        CameraStreamService cameraService;

        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            cameraService = new CameraStreamService();

            MessagingCenter.Subscribe<Application>(this, MessageSubject.CONNECTED.ToString(), (sender) =>
            {
                OnConnected();
            });

            MessagingCenter.Subscribe<Application>(this, MessageSubject.DISCONNECTED.ToString(), (sender) =>
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
            Container.RegisterTypeForNavigation<OnlineStreamMenuPage>();
            Container.RegisterTypeForNavigation<CameraPage>();
            Container.RegisterTypeForNavigation<GridPage>();
            
        }

        protected void OnConnected()
        {
            cameraService.Start();
        }

        protected void OnDisconnected()
        {
            cameraService.Dispose();
        }
    }
}
