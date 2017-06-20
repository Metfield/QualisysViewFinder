using Arqus.Helpers;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.Services
{

    /// <summary>
    ///   
    /// </summary>
    abstract class Stream<TData> : IDisposable
    {
        private ComponentType type;
        protected bool streaming;
        protected int frequency;

        // Variables to handle packets
        protected QTMNetworkConnection connection;

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

        

        protected async void ContinuousStream()
        {
            while(streaming)
            {
                try
                {
                    PacketType packetType;
                    connection.Protocol.ReceiveRTPacket(out packetType, false);
                    // Make sure this is a data packet
                    if (packetType == PacketType.PacketData)
                    {
                        RetrieveDataAsync(connection.Protocol.GetRTPacket());
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Debugger.Break();
                }
            }
        }

        protected abstract void RetrieveDataAsync(RTPacket packet);
        

        public void Dispose()
        {
            StopStream();
            connection.Dispose();
        }

        

    }
}

