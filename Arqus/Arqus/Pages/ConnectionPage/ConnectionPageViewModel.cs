using Arqus.Connection;
using Arqus.Services;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

using Acr.UserDialogs;
using System.Net;
using System.Net.Sockets;

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

            OnAboutButtonPressedCommand = new DelegateCommand(() => OnAboutButtonPressed());

            OnQualisysLinkTappedCommand = new DelegateCommand(() => Device.OpenUri(new Uri("http://www.qualisys.com/start/")));

            // Get LAN address
            // RUN ASYNCHRONOUSLY OR APP WILL CRASH WITH NO ERROR MESSAGE!
            Task.Run(() => IPAddress = GetLocalIPAddress());
        }
        
        public void OnNavigatedFrom(NavigationParameters parameters){ }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            if (!parameters.ContainsKey("__NavigationMode"))
                return;

            NavigationMode navigationMode = parameters.GetValue<NavigationMode>("__NavigationMode");

            // App was just started
            if (navigationMode == NavigationMode.New)
            {
                // Run the discovery command!
                // This shows and loads available servers in the network when starting the app
                Task.Run(() => ConnectionModeDiscoveryCommand.Execute());
            }
            else if (navigationMode == NavigationMode.Back)
            {
                connection.Dispose();

                // Make sure everything is clean
                SettingsService.Clean();

                GC.Collect();
            }
        }

        // Gets LAN IP and strips down the last byte
        private string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry("");
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (ip.ToString() == "127.0.0.1")
                        notificationService.Show("Warning", "Please make sure that you are connected to a network");

                    if (ip.GetAddressBytes()[0] != 192)
                    {
                        continue;
                    }

                    return ip.ToString().Remove(ip.ToString().LastIndexOf('.') + 1);
                }
            }

            // No ip was found.. weird!!
            return "";
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
                    selectedServer = null;
                }
            }
            get { return selectedServer; }
        }
        
        public DelegateCommand RefreshQTMServers { private set; get; }
        public DelegateCommand ConnectionModeDiscoveryCommand { private set; get; }
        public DelegateCommand ConnectionModeManuallyCommand { private set; get; }
        public DelegateCommand ConnectionModeDemoCommand { private set; get; }
        public DelegateCommand OnAboutButtonPressedCommand { private set; get; }
        public DelegateCommand OnQualisysLinkTappedCommand { private set; get; }

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
                List<QTMRealTimeSDK.DiscoveryResponse> DiscoveryResponse = connection.DiscoverQTMServers();


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
        
        private string ipAddress;

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
            // Bail if the user has already started a connection
            if (IsAttemptingConnection())
                return;

            try
            {
                // Connect to IP
                bool success = connection.Connect(IPAddress);

                if (!success)
                {
                    if(!QTMNetworkConnection.QTMVersionSupported)
                    {
                        // QTM is not supported
                        userDialogs.Alert("Please make sure that you are connecting to a Windows PC with " +
                                          "a Qualisys Track Manager software version of at least 2.16", "Attention", "Dismiss");
                    }
                    else
                    {
                        // There was an error with the connection
                        notificationService.Show("Attention", "There was a connection error, please check IP address");
                    }

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
                            .SetTitle("Please enter password (blank for slave mode)")
                            .SetOkText("Connect"));

                    if (!result.Ok)
                        return;
                    
                    // Connect to host
                    connection.Connect(IPAddress, result.Text);

                    if (!connection.TakeControl() && result.Text != "")
                    {
                        ToastAction toastAction = new ToastAction().SetText("Retry").SetAction(() => OnConnectionStarted());
                        ToastConfig toastConfig = new ToastConfig("Incorrect Password")
                            .SetAction(toastAction);
                        
                        userDialogs.Toast(toastConfig);
                        return;
                    }
                }

                // Send connection instance to settings service
                if(!SettingsService.Initialize())
                {
                    // There was a problem when attempting to establish the connection
                    userDialogs.Alert("There was a communication mismatch with QTM, please make sure you are running the" +
                                      " latest version of this application and that you have the QTM version specified in" +
                                      " the requirements", "Error", "Dismiss");

                    connection.Disconnect();
                    return;
                }

                // Show loading screen whilst connecting
                // This loading screen is disabled in the CameraPageViewModel constructor
                Task.Run(() => userDialogs.ShowLoading("Establishing connection..."));

                // Fetch information from system and fill structures accordingly
                CameraManager.GenerateCameras();

                // Connection was successfull                
                // Navigate to camera page
                NavigationParameters navigationParams = new NavigationParameters();
                navigationParams.Add(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING, false);
                Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("CameraPage", navigationParams));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                notificationService.Show("Attention", "Please make sure that QTM is up and running and that the cameras are plugged in");

                Task.Run(() => userDialogs.HideLoading());

                return;
            }
        }

        // Start app using demo mode
        void StartDemoMode()
        {
            // Bail if the user has already started a connection
            if (IsAttemptingConnection())
                return;

            // Show loading screen whilst connecting
            // This loading screen is disabled in the CameraPageViewModel constructor
            Task.Run(() => userDialogs.ShowLoading("Loading demo mode..."));

            // Initialize mock general settings
            SettingsService.Initialize(true);
            CameraManager.GenerateCameras();
            
            // Navigate to camera page
            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add(Helpers.Constants.NAVIGATION_DEMO_MODE_STRING, true);
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("CameraPage", navigationParams));
        }

        // Navigate to AboutPage when "i" icon has been pressed
        void OnAboutButtonPressed()
        {
            Device.BeginInvokeOnMainThread(() => navigationService.NavigateAsync("AboutPage"));
        }

        bool connectionPendant;

        // Provides protection when a connection routine has been started
        // Ignore more requests for 1 second, this include server selection,
        //  manual connection and demo mode
        bool IsAttemptingConnection()
        {
            if (connectionPendant)
                return true;

            Task.Run(() =>
            {
                connectionPendant = true;
                System.Threading.Thread.Sleep(1000);
                connectionPendant = false;
            });

            return false;
        }
    }
}

