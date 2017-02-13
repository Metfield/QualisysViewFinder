using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using Xamarin.Forms;

using System.Net;

namespace ArqusPrototype
{
	public class App : Application
	{
        Entry entryField;
        public Label frameLabel;
        protected QTMRealTimeSDK.RTProtocol rtProtocol;
        string serverAddress;
        string qtmVersion;
        string frameString = "Frames a comin'!";

        public App()
        {
            // Specify UI elements
            Label greetingLabel = new Label
            {

                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Start,
                Text = "Please specify an IP address to connect to"
            };

            entryField = new Entry
            {
                Placeholder = "IPv4",
                HorizontalTextAlignment = TextAlignment.Center,
            };

            Button connectButton = new Button
            {
                Text = "Connect",
                HorizontalOptions = LayoutOptions.Center
            };

            frameLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Olive,
                Text = frameString
            };


            // Add to callback method
            connectButton.Clicked += OnConnectButtonTouched;

            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        greetingLabel,
                        entryField,
                        connectButton
                    }
                }
            };
        }

        // Button callback method
        void OnConnectButtonTouched(object sender, EventArgs e)
        {
            string ipAddress = entryField.Text;

            SharedUtils.Log("Address: " + entryField.Text);

            // Check if this is a valid IP address
            if (IsIPv4(ipAddress))
            {
                // Create rtProtocol object
                rtProtocol = new QTMRealTimeSDK.RTProtocol();

                // Store vallid server address
                this.serverAddress = ipAddress;

                // Connect to valid IP
                ConnectToIP(ipAddress);

                // Get QTM version                
                rtProtocol.GetQTMVersion(out qtmVersion);

                // Change page layout and display QTM version
                MainPage = new ContentPage
                {
                    Content = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                HorizontalTextAlignment = TextAlignment.Center,
                                VerticalTextAlignment = TextAlignment.Start,
                                Text = "Connected! "  + qtmVersion
                            },
                            frameLabel
                        }
                    }
                };

                StartStreaming();
            }
            else
            {
                SharedUtils.ShowNotification("Please enter a valid IP Address");
            }
        }

        // Start QTM frame streaming
        async Task StartStreaming()
        {
            SharedUtils.Log("StartStreaming.. ");

            while (true)
                await StreamFrames();
        }

        async Task StreamFrames()
        {
            // Check for available 3D data
            if (rtProtocol.Settings3D == null)
            {
                if (!rtProtocol.Get3dSettings()) // NO NO!
                {
                    SharedUtils.Log("QTM: Trying to get 3D settings");

                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    return;
                }

                SharedUtils.Log("QTM: 3D data available");

                rtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component3dResidual);
                SharedUtils.Log("QTM: Starting to stream 3D data");

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            // Get RTPacket from stream
            QTMRealTimeSDK.Data.PacketType packetType;
            rtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == QTMRealTimeSDK.Data.PacketType.PacketData)
            {
                var threeDData = rtProtocol.GetRTPacket().Get3DMarkerResidualData();
                if (threeDData != null)
                {
                    // Display Frame number                                 
                    frameString = "Frame " + rtProtocol.GetRTPacket().Frame;
                    SharedUtils.Log(frameString);

                    // Go through the markers
                    /*  for (int body = 0; body < threeDData.Count; body++)
                    {
                        var threeDBody = threeDData[body];
                        
                        /*SharedUtils.Log("Frame: " + rtProtocol.GetRTPacket().Frame +
                                        " Body: " + bodySetting.Name +
                                           " X: " + threeDBody.Position.X + " X: " + threeDBody.Position.X + " Y: " + threeDBody.Position.Y + " Z: " + threeDBody.Position.Z +                                            
                                            " Residual: " + threeDBody.Residual);
                    }*/
                }
            }

            // Handle event packet
            if (packetType == QTMRealTimeSDK.Data.PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = rtProtocol.GetRTPacket().GetEvent();
                SharedUtils.Log("Event: " + qtmEvent);
            }
        }

        private void Set(Func<object> p, ref string frameString, string value)
        {
            throw new NotImplementedException();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            SharedUtils.Log("OnStart Homie!");
        }

        public string FrameInfo
        {
            get { return frameString; }
            set { Set(() => FrameInfo, ref frameString, value); }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public bool ConnectToIP(string iPAddress)
        {
            if (!rtProtocol.IsConnected())
            {
                // Can crash if shitty address
                if (!rtProtocol.Connect(iPAddress))
                {
                    SharedUtils.Log("Could not connect to server");
                    SharedUtils.ShowNotification("Could not find server, please check IP");

                    return false;
                }
            }

            SharedUtils.Log("QTM: Connected");
            return true;
        }

        public bool IsIPv4(string ipString)
        {
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

            return false;
        }
    }
}
