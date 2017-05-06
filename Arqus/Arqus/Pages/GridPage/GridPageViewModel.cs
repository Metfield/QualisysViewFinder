using Arqus.Helpers;
using Arqus.Service;
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
            // MessagingCenterService.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString())

            MessagingCenter.Subscribe<Application, int>(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), OnNavigateToCameraPage);
        }

        void OnNavigateToCameraPage(Application sender, int cameraID)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                /*NavigationParameters parameters = new NavigationParameters()
                {
                    { "toCameraPage", true }
                };*/
                
                navigationService.NavigateAsync("CameraPage");
            });
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            try
            {
                NavigationMode navigationMode = (NavigationMode)parameters["__NavigationMode"];

                if ( navigationMode == NavigationMode.Back)
                    MessagingService.Send(Application.Current, MessageSubject.DISCONNECTED.ToString(), new { Poop = "poop" });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {

        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }
    }
}
