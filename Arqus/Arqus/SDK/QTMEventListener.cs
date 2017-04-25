using Arqus.Helpers;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    public class QTMEventListener : IDisposable
    {
        private int frequency;
        private QTMNetworkConnection networkConnection;
        private bool isStreaming;

        public QTMEventListener(int freq, bool startListening = true)
        {
            // Init variables
            frequency = freq;
            isStreaming = false;

            // TODO: Mmmph.. new connection?
            networkConnection = new QTMNetworkConnection();

            // Initialize with frequency
            networkConnection.Protocol.StreamFrames(QTMRealTimeSDK.StreamRate.RateFrequency, frequency);

            // Start listening if necessary
            if (startListening)
                StartListening();
        }

        public void StartListening()
        {
            if (!networkConnection.Protocol.IsConnected())
                networkConnection.Connect();

            Task.Run(() => ListenToEvents());
        }

        private void ListenToEvents()
        {
            PacketType packetType = new PacketType();

            // System is getting double event packages..
            // For now just ignore one.. 
            //TODO: Fix this issue!
            bool ignoreNextPacket = false;

            while (networkConnection.Protocol.IsConnected())
            {
                // Get Packet and don't skip events
                networkConnection.Protocol.ReceiveRTPacket(out packetType, false);

                // Check if this is an event packet
                if (packetType == PacketType.PacketEvent)
                {
                    // Check if camera settings have changed
                    if (networkConnection.Protocol.GetRTPacket().GetEvent() == QTMEvent.EventCameraSettingsChanged)
                    {
                        if (ignoreNextPacket)
                        {
                            ignoreNextPacket = false;
                            continue;
                        }

                        MessagingCenter.Send(this, MessageSubject.CAMERA_SETTINGS_CHANGED.ToString());
                        ignoreNextPacket = true;
                    }
                }
            }
        }

        public void StopListening()
        {
            networkConnection.Disconnect();
        }

        public void Dispose()
        {
            networkConnection.Protocol.StreamFramesStop();
            networkConnection.Disconnect();
            networkConnection.Dispose();
        }
    }
}
