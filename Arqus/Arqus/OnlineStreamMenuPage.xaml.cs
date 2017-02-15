using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Arqus
{
	public partial class OnlineStreamMenuPage : ContentPage
	{
		public OnlineStreamMenuPage ()
		{
			InitializeComponent ();

            // Get qtm version and set it to label
            qtmVersion.Text = ((App)App.Current).getCameraStream().GetQTMVersion();
		}

        // "Stream" button has been pressed
        void OnStreamButtonClicked(object sender, EventArgs args)
        {
            SharedProjects.Notification.Show("HURRAY!", "STREAM PRESSED!");
        }

    }
}
