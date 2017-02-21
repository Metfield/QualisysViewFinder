using System;
using System.Collections.Generic;
using System.Text;
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

        /// GUI stream start callback
        void OnStream2DCommand()
        {
            SharedProjects.Notification.Show("YO!", "2D streaming has just begun!");
            CameraStream.Instance.StartStream(QTMRealTimeSDK.Data.ComponentType.Component2d);
        }

        void OnStream3DCommand()
        {
            SharedProjects.Notification.Show("YO!", "3D streaming has just begun!");
            CameraStream.Instance.StartStream(QTMRealTimeSDK.Data.ComponentType.Component3d);
        }

        //// QtmVersion string binding to text label
        public string QtmVersion
        {
            get { return qtmVersion; }
        }
    }
}
