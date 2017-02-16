using System;
using System.Net;
using Xamarin.Forms;

using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System.Collections.Generic;
using System.Diagnostics;

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

            /*
            RTProtocol rt = new RTProtocol();
            QTMNetworkConnection qtm = new QTMNetworkConnection(ref rt);
            ushort port = 4547;
            List<DiscoveryResponse> response = qtm.DiscoverQTMServers(port);

            foreach (var server in response)
            {
                Debug.WriteLine(server.IpAddress);
            }

            Debug.WriteLine("Finished discovery");*/
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
