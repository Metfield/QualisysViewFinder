using Prism.Unity;
using Xamarin.Forms;

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
            Container.RegisterTypeForNavigation<NavigationPage>();
            Container.RegisterTypeForNavigation<ConnectionPage>();
            Container.RegisterTypeForNavigation<OnlineStreamMenuPage>();
            Container.RegisterTypeForNavigation<MarkerPage>();
        }


    }
}
