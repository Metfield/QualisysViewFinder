using Arqus.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arqus
{
    class ConnectionViewModel : ViewModel
    {
        QTMServer selectedServer;
        IEnumerable<QTMServer> qtmServers;
        private QTMNetworkConnection networkConnection = new QTMNetworkConnection();

        public ConnectionViewModel()
        {
            qtmServers = networkConnection.DiscoverQTMServers().Select(server => new QTMServer(server.IpAddress,
                        server.HostName,
                        server.Port.ToString(),
                        server.InfoText,
                        server.CameraCount.ToString())
                        );
        }

        /// <summary>
        /// The selected server to connect to
        /// </summary>
        public QTMServer SelectedServer {
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
        
    }
}
