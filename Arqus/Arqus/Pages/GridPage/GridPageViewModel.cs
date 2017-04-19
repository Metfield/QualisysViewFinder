using Arqus.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
    class GridPageViewModel : BindableBase, INavigationAware
    {
        private INavigationService navigationService;
        public DelegateCommand NavigateCameraViewCommand { private set; get; }

        public GridPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            //NavigateCameraViewCommand = new DelegateCommand(() => OnNavigateToCameraPage());
            // MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString())

            MessagingCenter.Subscribe<Application, int>(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), OnNavigateToCameraPage);
        }

        void OnNavigateToCameraPage(Application sender, int cameraID)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CameraStore.State.ID = cameraID;
                navigationService.NavigateAsync("CameraPage");
            });
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
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
