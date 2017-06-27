using Arqus.Connection;
using Arqus.Services;
using Arqus.Services.MobileCenterService;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;

using QTMRealTimeSDK.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using QTMRealTimeSDK.Settings;
using Acr.UserDialogs;

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
        private PromptConfig passwordPromptConfig;

        private IUserDialogs userDialogs;
        private IUnityContainer container;
        private INavigationService navigationService;
        private INotification notificationService;

        public ConnectionPageViewModel(
            INavigationService navigationService, 
            IUnityContainer container, 
            INotification notification,
            IUserDialogs userDialogs)
        {
            this.userDialogs = userDialogs;
            this.container = container;
            this.navigationService = navigationService;
            this.notificationService = notification;

            connection = new QTMNetworkConnection();
            CurrentConnectionMode = ConnectionMode.NONE;

            RefreshQTMServers = new DelegateCommand(() => Task.Run(() => LoadQTMServers()));


            ConnectionModeDiscoveryCommand = new DelegateCommand(() => {
                LoadQTMServers();
                IsDiscovery = true;
            }).ObservesCanExecute(() => IsDiscoverButtonEnabled);

            ConnectionModeManuallyCommand = new DelegateCommand(() => { IsManually = true; }).ObservesCanExecute(() => IsManualButtonEnabled);
            ConnectionModeDemoCommand = new DelegateCommand(() => Task.Run(() => StartDemoMode()));

            ConnectCommand = new DelegateCommand(() => Task.Run(() => OnConnectionStarted()));
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            MobileCenterService.TrackEvent(GetType().Name, "NavigatedFrom");
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {

            MobileCenterService.TrackEvent(GetType().Name, "NavigatedTo");

            try
            {
                NavigationMode navigationMode = parameters.GetValue<NavigationMode>("NavigationMode");

                if (navigationMode == NavigationMode.Back)
                {
                    connection.Dispose();

                    // Make sure everything is clean
                    SettingsService.Clean();
                    //CameraStore.Clean();

                    //GC.Collect();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }            
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
        public DelegateCommand ConnectionModeDemoCommand { private set; get; }

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

        IEnumerable<QTMServer> qtmServers;

        /// <summary>
        /// The list of running QTMServers on the network
        /// </summary>
        public IEnumerable<QTMServer> QTMServers
        {
            set { if (SetProperty(ref qtmServers, value)) IsRefreshing = false; }
            get { return qtmServers; }
        }

        /// <summary>
        /// Makes a discovery of nearby QTM Servers and updates the known locations accordingly
        /// During the discovery the list will be in a refresh state
        /// </summary>
        public Task<IEnumerable<QTMServer>> LoadQTMServersAsync()
        {
            return Task.Run(() =>{
                // BUG: The application will crash upon a second refresh
                // JNI ERROR (app bug): attempt to use stale local reference 0x100019 (should be 0x200019)
                List<QTMRealTimeSDK.RTProtocol.DiscoveryResponse> DiscoveryResponse = connection.DiscoverQTMServers();


                return DiscoveryResponse.Select(server => new QTMServer(server.IpAddress,
                            server.HostName,
                            server.Port.ToString(),
                            server.InfoText,
                            server.CameraCount.ToString())
                            );
            });
        }
            

        public async void LoadQTMServers()
        {
            IsRefreshing = true;
            QTMServers = await LoadQTMServersAsync();
        }


        // Command button binding        
        public DelegateCommand ConnectCommand { private set;  get; }
        
        private string ipAddress = "192.168.10.179";

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
            try
            {
                // Connect to IP

                bool success = connection.Connect(IPAddress);

                if (!success)
                {
                    // There was an error with the connection
                    //SharedProjects.Notification.Show("Attention", "There was a connection error, please check IP and pass");
                    notificationService.Show("Attention: There was a connection error, please check IP address");
                    return;
                }
                else if(connection.HasPassword())
                {
                    // NOTE: A new propmt configuration has to be created everytime we run it
                    // if we do not do this the acr library will add an action to it and since
                    // it is async it does not expect that and will thus throw an error
                    PromptResult result = await userDialogs
                        .PromptAsync(
                            new PromptConfig()
                            .SetTitle("Please enter a password")
                            .SetOkText("Connect")
                            );

                    connection.Connect(IPAddress, result.Text);

                    if (!connection.TakeControl())
                    {
                        userDialogs.Toast(new ToastConfig("Incorrect Password"));
                        return;
                    }
                }


                // Send connection instance to settings service
                SettingsService.Initialize();
                CameraStore.GenerateCameras();

                // Connection was successfull                
                // Navigate to camera page
                NavigationParameters navigationParams = new NavigationParameters();
                navigationParams.Add(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING, false);
                Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("CameraPage", navigationParams));

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                notificationService.Show("Please make sure that QTM is up and running");

                return;
            }
        
        }

        // Start app using demo mode
        void StartDemoMode()
        {
            // Initialize mock general settings
            SettingsService.Initialize(true);
            CameraStore.GenerateCameras();
            
            // Navigate to camera page
            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING, true);
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("CameraPage", navigationParams));
        }
    }      
}

