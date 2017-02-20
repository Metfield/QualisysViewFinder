using Arqus.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    }
}
