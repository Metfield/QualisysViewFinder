using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Xamarin.Forms;


namespace Arqus
{
    class OnlineStreamMenuViewModel : ViewModel
    {
        string qtmVersion;        

        public OnlineStreamMenuViewModel()
        {
            // Bind commands to methods
            Stream2DCommand = new Command(OnStream2DCommand);
            Stream3DCommand = new Command(OnStream3DCommand);
            
            // Get QTM version
            qtmVersion = CameraStream.Instance.GetQTMVersion();
        }

        // OnlineStreamMenuPage.xaml bindings         
        //// Stream 2D button command
        public Command Stream2DCommand { get; }
        public Command Stream3DCommand { get; }

        //// GUI Start 2D stream button callback
        void OnStream2DCommand()
        {            
            CameraStream.Instance.StartStream(70, QTMRealTimeSDK.Data.ComponentType.Component2d);
            // Switch to Tracking2D page
            ((App)App.Current).MainPage = new Tracking2DPage();
        }

        //// GUI Start 3D stream button callback
        void OnStream3DCommand()
        {            
            // Initialize streaming in Component3D mode
            CameraStream.Instance.StartStream(70, QTMRealTimeSDK.Data.ComponentType.Component3d);

            // Switch to Tracking3D page
            ((App)App.Current).MainPage = new Tracking3DPage();            
        }

        //// QtmVersion string binding to text label
        public string QtmVersion
        {
            get { return qtmVersion; }
        }
    }
}
