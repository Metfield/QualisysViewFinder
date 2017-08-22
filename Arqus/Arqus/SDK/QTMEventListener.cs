
using QTMRealTimeSDK.Data;
using System;

using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus
{
    public class QTMEventListener : IDisposable
    {
        private int frequency;
        private QTMNetworkConnection networkConnection;   

        public QTMEventListener(int freq, bool startListening = true)
        {
            // Init variables
            frequency = 30;

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
                networkConnection.Connect(networkConnection.GetRandomPort());

            Task.Run(() => ListenToEvents());
        }


        private void ListenToEvents()
        {
            // System is getting double event packages..
            // For now just ignore one.. 
            //TODO: Fix this issue!
            bool ignoreNextPacket = false;
            QTMEvent eventPacket;

            while (networkConnection.Protocol.IsConnected())
            {
                PacketType packetType;
                // Get Packet and don't skip events
                networkConnection.Protocol.ReceiveRTPacket(out packetType, false);

                // Check if this is an event packet
                if (packetType == PacketType.PacketEvent)
                {
                    eventPacket = networkConnection.Protocol.GetRTPacket().GetEvent();

                    // Check if camera settings have changed
                    if (eventPacket == QTMEvent.EventCameraSettingsChanged && !QTMNetworkConnection.ConnectionIsRecordedMeasurement)
                    {
                        MessagingCenter.Send(this, Messages.Subject.CAMERA_SETTINGS_CHANGED);
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
