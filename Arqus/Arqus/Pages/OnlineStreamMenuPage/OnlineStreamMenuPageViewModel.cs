using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Xamarin.Forms;


namespace Arqus
{
    class OnlineStreamMenuPageViewModel : BindableBase
    {
        string qtmVersion;
        private INavigationService _navigationService;

        public OnlineStreamMenuPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // Bind commands to methods
            Stream2DCommand = new DelegateCommand(OnStream2DCommand);
            
            // Get QTM version
            qtmVersion = QTMNetworkConnection.Version;
        }

        // OnlineStreamMenuPage.xaml bindings         
        //// Stream 2D button command
        public DelegateCommand Stream2DCommand { get; }
        public DelegateCommand Stream3DCommand { get; }

        //// GUI Start 2D stream button callback
        void OnStream2DCommand()
        {

            if (CameraStream.Instance.StartStream(1, QTMRealTimeSDK.Data.ComponentType.ComponentImage))
            {
                // Switch to Tracking2D page
                _navigationService.NavigateAsync("CameraPage");
            }
        }

        //// QtmVersion string binding to text label
        public string QtmVersion
        {
            get { return qtmVersion; }
        }
    }
}
