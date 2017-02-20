using Arqus.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arqus
{
    class ConnectionViewModel : ViewModel
    {
        private QTMNetworkConnection networkConnection = new QTMNetworkConnection();

        // Todo: Make implement selected server so that ViewModel works independently from Xamarin.Forms
        QTMServer selectedServer;

        /// <summary>
        /// The selected server to connect to
        /// </summary>
        public QTMServer SelectedServer {
            private set { SetProperty(ref selectedServer, value); }
            get
            {
                return selectedServer;
            }
        }

        IEnumerable<QTMServer> qtmServers;

        /// <summary>
        /// The list of running QTMServers on the network
        /// </summary>
        public IEnumerable<QTMServer> QTMServers
        {
            private set
            { SetProperty(ref qtmServers, value); }
            get
            {

                return qtmServers;
            }
        }

        bool isLoading;

        public bool IsLoading
        {
            private set{ SetProperty(ref isLoading, value); }
            get
            {
                return isLoading;
            }
        }

        public bool IsDoneLoading
        {
            get
            {
                Debug.WriteLine("Return done loading");
                return !isLoading;
            }
        }

        public async void RefreshQTMServers()
        {
            IsLoading = true;
            List<QTMRealTimeSDK.RTProtocol.DiscoveryResponse> DiscoveryResponse = await Task.Run(() => networkConnection.DiscoverQTMServers());
            QTMServers = DiscoveryResponse.Select(server => new QTMServer(server.IpAddress,
                        server.HostName,
                        server.Port.ToString(),
                        server.InfoText,
                        server.CameraCount.ToString())
                        );
            IsLoading = false;
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
