using Arqus.Connection;
using Arqus.Services.MobileCenterService;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    class ConnectionPageViewModel : BindableBase, INavigatedAware
    {
        public enum ConnectionMode
        {
            DISCOVER,
            MANUALLY,
            NONE
        }
        
        private QTMNetworkConnection connection;

        private IUnityContainer container;
        private INavigationService navigationService;

        public ConnectionPageViewModel(
            INavigationService navigationService, 
            IUnityContainer container, 
            IPageDialogService pageDialogService)
        {
            this.container = container;
            this.navigationService = navigationService;

            connection = new QTMNetworkConnection();
            CurrentConnectionMode = ConnectionMode.NONE;

            RefreshQTMServers = new DelegateCommand(() => Task.Run(() => LoadQTMServers()));
            
            ConnectionModeDiscoveryCommand = new DelegateCommand(() => { IsDiscovery = true; }).ObservesCanExecute(() => IsDiscoverButtonEnabled);
            ConnectionModeManuallyCommand = new DelegateCommand(() => { IsManually = true; }).ObservesCanExecute(() => IsManualButtonEnabled);
            ConnectCommand = new DelegateCommand(() => Task.Run(() => OnConnectionStarted()));

            
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedFrom");
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedTo");
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
            List<QTMRealTimeSDK.RTProtocol.DiscoveryResponse> DiscoveryResponse = connection.DiscoverQTMServers();


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


        private string ipAddress = "192.168.10.168";

        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }
        
        private string password = "test";

        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        /// <summary>
        /// Callback method for starting connection
        /// </summary>
        async void OnConnectionStarted()
        {
            // Connect to IP
            bool success = connection.Connect(IPAddress, Password);

            if (!success)
            {
                // There was an error with the connection
                //SharedProjects.Notification.Show("Attention", "There was a connection error, please check IP and pass");
                Debug.Print("Attention: There was a connection error, please check IP and pass");
                return;
            }

            // 
            //networkConnection.Dispose();

            // Send connection instance to settings service
            SettingsService.Initialize();
            CameraStore.GenerateCameras();

            // Connection was successfull          
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("CameraPage"));
        }
    }
}
