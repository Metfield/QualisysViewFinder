using Arqus.Helpers;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.Services
{

    /// <summary>
    /// Description: It is meant to retrieve RT packets in a 
    /// consistent way    
    /// </summary>
    abstract class Stream<TData> : IDisposable
    {
        private ComponentType type;
        protected bool streaming;
        protected int frequency;

        // Variables to handle packets
        protected QTMNetworkConnection connection;
        static int streamCount;

        protected Stream(ComponentType type, int frequency)
        {
            this.type = type;
            this.frequency = frequency;
            connection = new QTMNetworkConnection();
        }

        public void StartStream()
        {
            if (!streaming)
            {
                streaming = true;
                if (connection.Protocol.StreamFrames(StreamRate.RateFrequency, frequency, type))
                    Task.Run(() => ContinuousStream());
            }
            else
            {
                Debug.WriteLine("Already streaming: ", type.ToString());
            }
        }



        public void StopStream()
        {
            streaming = false;
            connection.Protocol.StreamFramesStop();

        }

        RTPacket packet;
        /// <summary>
        /// Description: Streams data in the same frequency as the initiated stream
        /// with the QTM server. If it manages to process packets faster that expected
        /// it will wait.
        /// </summary>
        protected void ContinuousStream()
        {
            PacketType packetType;
            connection.Protocol.ReceiveRTPacket(out packetType);
            // Make sure this is a data packet
            if (packetType == PacketType.PacketData)
            {
                RetrieveDataAsync(connection.Protocol.GetRTPacket());
            }
            if(streaming)
                ContinuousStream();
        }
        
        protected abstract void RetrieveDataAsync(RTPacket packet);
        

        public void Dispose()
        {
            StopStream();
            connection.Protocol.Dispose();
        }
        
    }
}

