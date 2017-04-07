using Arqus.Connection;
using Arqus.Services;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    class ConnectionPageViewModel : BindableBase
    {
        public enum ConnectionMode
        {
            DISCOVER,
            MANUALLY,
            NONE
        }
        
        private QTMNetworkConnection networkConnection;
        private INavigationService navigationService;
        private IUnityContainer container;

        public ConnectionPageViewModel(INavigationService navigationService, IUnityContainer container)
        {
            this.container = container;
            this.navigationService = navigationService;

            networkConnection = new QTMNetworkConnection();
            CurrentConnectionMode = ConnectionMode.NONE;

            RefreshQTMServers = new DelegateCommand(() => Task.Run(() => LoadQTMServers()));
            
            ConnectionModeDiscoveryCommand = new DelegateCommand(() => { IsDiscovery = true; }).ObservesCanExecute(() => IsDiscoverButtonEnabled);
            ConnectionModeManuallyCommand = new DelegateCommand(() => { IsManually = true; }).ObservesCanExecute(() => IsManualButtonEnabled);
            ConnectCommand = new DelegateCommand(() => OnConnectionStarted());
        }
        

       QTMServer selectedServer = null;

        /// <summary>
        /// The selected server to connect to
        /// </summary>
        public QTMServer SelectedServer
        {
            set{
                if (SetProperty(ref selectedServer, value))
                {
                    IPAddress = selectedServer.IPAddress;
                    OnConnectionStarted();
                }
            }
            get { return selectedServer; }
        }
        
        public DelegateCommand RefreshQTMServers { private set; get; }
        public DelegateCommand ConnectionModeDiscoveryCommand { private set; get; }
        public DelegateCommand ConnectionModeManuallyCommand { private set; get; }

        private ConnectionMode currentConnectionMode;

        public ConnectionMode CurrentConnectionMode
        {
            get { return currentConnectionMode; }
            set { Debug.WriteLine(value);  SetProperty(ref currentConnectionMode, value); }
        }


        bool isDiscovery;
        public bool IsDiscovery
        {
            set
            { 
                if(value)
                {
                    CurrentConnectionMode = ConnectionMode.DISCOVER;
                    IsDiscoverButtonEnabled = false;
                    IsManually = false;
                }
                else
                {
                    IsDiscoverButtonEnabled = true;
                }

                SetProperty(ref isDiscovery, value);
            }
            get
            {
                return CurrentConnectionMode == ConnectionMode.DISCOVER;
            }
        }

        private bool isDiscoverButtonEnabled = true;

        public bool IsDiscoverButtonEnabled
        {
            get { return isDiscoverButtonEnabled; }
            set { SetProperty(ref isDiscoverButtonEnabled, value); }
        }


        bool isManually;
        public bool IsManually
        {
            set
            {
                if (value)
                {
                    CurrentConnectionMode = ConnectionMode.MANUALLY;
                    IsManualButtonEnabled = false;
                    IsDiscovery = false;
                }
                else
                {
                    IsManualButtonEnabled = true;
                }

                SetProperty(ref isManually, value);
            }
            get
            {
                return isManually;
            }
        }

        private bool isManualButtonEnabled = true;

        public bool IsManualButtonEnabled
        {
            get { return isManualButtonEnabled; }
            set { SetProperty(ref isManualButtonEnabled, value); }
        }

        public void SetConnectionMode(ConnectionMode mode)
        {
            CurrentConnectionMode = mode;
        }

        bool isRefreshing;

        public bool IsRefreshing
        {
            set{ SetProperty(ref isRefreshing, value); }
            get{ return isRefreshing; }
        }

        ObservableCollection<QTMServer> qtmServers;

        /// <summary>
        /// The list of running QTMServers on the network
        /// </summary>
        public ObservableCollection<QTMServer> QTMServers{ private set; get; }

        /// <summary>
        /// Makes a discovery of nearby QTM Servers and updates the known locations accordingly
        /// During the discovery the list will be in a refresh state
        /// </summary>
        public IEnumerable<QTMServer> LoadQTMServersAsync()
        {
            // BUG: The application will crash upon a second refresh
            // JNI ERROR (app bug): attempt to use stale local reference 0x100019 (should be 0x200019)
            List<QTMRealTimeSDK.RTProtocol.DiscoveryResponse> DiscoveryResponse = networkConnection.DiscoverQTMServers();


            return DiscoveryResponse.Select(server => new QTMServer(server.IpAddress,
                        server.HostName,
                        server.Port.ToString(),
                        server.InfoText,
                        server.CameraCount.ToString())
                        ); ;
        }

        public async void LoadQTMServers()
        {
            IsRefreshing = true;
            IEnumerable<QTMServer> servers = LoadQTMServersAsync();
            
            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var server in servers)
                {
                    QTMServers.Add(server);
                }
            });

            IsRefreshing = false;
        }


        // Command button binding        
        public DelegateCommand ConnectCommand { private set;  get; }


        private string ipAddress = "192.168.10.170";

        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }
        
        
        /// <summary>
        /// Callback method for starting connection
        /// </summary>
        void OnConnectionStarted()
        {
            // Check if ip is valid
            if (!IsValidIPv4(IPAddress))
            {
                SharedProjects.Notification.Show("Attention", "Please enter a valid IP address");                
                return;
            }

            // Proceed to connect to address
            ConnectAsync(IPAddress);
        }

        /// <summary>
        /// Starts network communication with QTM through specified IP
        /// </summary>
        /// <param name="ipAddress">QTM's instance address</param>
        public async void ConnectAsync(string ipAddress)
        {
            // Connect to IP
            if (!networkConnection.Connect(ipAddress))
            {
                // There was an error with the connection
                SharedProjects.Notification.Show("Attention", "There was a connection error, please check IP");
                return;
            }
            
            container.RegisterType<ISettingsService, SettingsService> (new InjectionConstructor(ipAddress));
            // Connection was successfull          
            // Begin streaming 
            await navigationService.NavigateAsync("OnlineStreamMenuPage");
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
            if (System.Net.IPAddress.TryParse(ipString, out address))
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
