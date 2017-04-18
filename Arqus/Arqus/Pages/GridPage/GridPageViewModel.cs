using Arqus.Helpers;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    class GridPageViewModel : INavigationAware
    {
        private INavigationService navigationService;

        public GridPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            MessagingCenter.Subscribe<GridApplication, int>(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), OnNavigateToCameraPage);
        }

        async void OnNavigateToCameraPage(GridApplication application, int cameraID)
        {
            //await navigationService.GoBackAsync();
            //UrhoSurface.OnDestroy();
            UrhoSurface.OnDestroy();
            await navigationService.NavigateAsync("CameraPage");
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            //UrhoSurface.OnDestroy();
            Debug.WriteLine("NAVIGATING FROM");
            //MessagingCenter.Send(Application.Current, MessageSubject.DISCONNECTED.ToString());
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {

        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            MessagingCenter.Send(Application.Current, MessageSubject.CONNECTED.ToString());
        }
    }
}
