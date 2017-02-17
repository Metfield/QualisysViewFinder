using Arqus.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace Arqus
{
    class ConnectionViewModel : ViewModel
    {
        QTMServer selectedServer;
        IEnumerable<QTMServer> qtmServers;
        private QTMNetworkConnection networkConnection = new QTMNetworkConnection();

        string connectionIPAddress;

        public ConnectionViewModel()
        {
            qtmServers = networkConnection.DiscoverQTMServers().Select(server => new QTMServer(server.IpAddress,
                        server.HostName,
                        server.Port.ToString(),
                        server.InfoText,
                        server.CameraCount.ToString())
                        );

            // Add button command to binding context
            ConnectCommand = new Command(OnConnectionStarted);
        }

        /// <summary>
        /// The selected server to connect to
        /// </summary>
        public QTMServer SelectedServer
        {
            set
            {
                if(selectedServer != value)
                {
                    OnPropertyChanged();
                }
            }
            get
            {
                return selectedServer;
            }
        }

        /// <summary>
        /// The list of running QTMServers on the network
        /// </summary>
        public IEnumerable<QTMServer> QTMServers
        {
            set
            {
                if(qtmServers != value)
                {
                    OnPropertyChanged();
                }
            }
            get
            {
                return qtmServers;
            }
        }

        
        // Command button binding        
        public Command ConnectCommand { get; }     

        // IP address text field binding 
        public string ConnectionIPAddress
        {            
            set { connectionIPAddress = value; }
        }
        
        /// <summary>
        /// Callback method for starting connection
        /// </summary>
        void OnConnectionStarted()
        {
            // Get ip string from field
            string ipAddress = connectionIPAddress;

            // Check if ip is valid
            if (!IsValidIPv4(ipAddress))
            {
                SharedProjects.Notification.Show("Attention", "Please enter a valid IP address");                
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
            if (ipString == null)
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
