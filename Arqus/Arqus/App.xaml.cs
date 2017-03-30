using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Arqus
{
	public partial class App : Application
	{
        CameraStream cameraStream;        

		public App ()
		{
			InitializeComponent();
			MainPage = new Arqus.ConnectionPage();
		}

		protected override void OnStart ()
		{
            // Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
            // Handle when your app resumes            
        }

        /// <summary>
        /// Starts network communication with QTM through specified IP
        /// </summary>
        /// <param name="ipAddress">QTM's instance address</param>
        public void Connect(string ipAddress)
        {
            // Get CameraStream instance and init real-time protocol
            cameraStream = CameraStream.Instance;

            // Connect to IP
            if (!cameraStream.ConnectToIP(ipAddress))
            {
                // There was an error with the connection
                SharedProjects.Notification.Show("Attention", "There was a connection error, please check IP");
                return;
            }

            // Connection was successfull          
            // Begin streaming 
            //MainPage = new OnlineStreamMenuPage();             

            // Load Grid view instead
            MainPage = new GridPage();
        }

        public CameraStream getCameraStream()
        {
            return cameraStream;            
        }
    }
}
