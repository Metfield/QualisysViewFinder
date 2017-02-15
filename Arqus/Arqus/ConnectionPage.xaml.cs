using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
	public partial class ConnectionPage : ContentPage
	{
		public ConnectionPage()
		{
			InitializeComponent();
		}

        // "Connect" button was pressed
        void OnConnectionButtonClicked(object sender, EventArgs args)
        {
            // Get ip string from field
            string ipAddress = ipEntryField.Text;

            // Check if ip is valid
            if (!IsValidIPv4(ipAddress))
            {
                DisplayAlert("Attention", "Please enter a valid IP address", "OK");
                return;
            }

            // Proceed to connect to address
            // Delegate work to application's main class (App)
            // Need to cast Current as App
            ((App)App.Current).Connect(ipAddress);            
        }

        /// <summary>
        /// Makes sure ipAddress string is a valid IPv4
        /// </summary>
        /// <param name="ipString">Holds QTM instance IP address</param>
        /// <returns></returns>
        public bool IsValidIPv4(string ipString)
        {
            // Check for null string
            if(ipString == null)            
                return false;            

            // Check if it's made of four elements
            if (ipString.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length != 4)
                return false;

            IPAddress address;

            // Check if this is a valid IP address
            if (IPAddress.TryParse(ipString, out address))
            {
                // Make sure it's an ipv4 (although it should)
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            // TODO: Check if address is in LAN and in a valid range!

            return false;
        }
    }
}
