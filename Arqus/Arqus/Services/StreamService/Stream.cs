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

        private bool demoMode;

        // Variables to handle packets
        protected QTMNetworkConnection connection;

        // This task handles do ContinuousStreaming loop
        Task streamTask;

        protected Stream(ComponentType type, int frequency, bool demoMode)
        {
            this.type = type;
            this.frequency = frequency;
            this.demoMode = demoMode;

            if(!demoMode)
                connection = new QTMNetworkConnection();
        }

        public void StartStream()
        {
            if (!streaming)
            {
                streaming = true;

                if (!demoMode)
                {
                    if (connection.Protocol.StreamFrames(StreamRate.RateFrequency, frequency, type))
                        streamTask = Task.Run(() => ContinuousStream());
                }
                else
                {
                    // TODO: Assuming DemoStream is going great!
                    streamTask = Task.Run(() => ContinuousStream());
                }
            }
            else
            {
                Debug.WriteLine("Already streaming: ", type.ToString());
            }
        }

        public void StopStream()
        {
            streaming = false;
            
            if (!demoMode)
                connection.Protocol.StreamFramesStop();
        }
        
        PacketType packetType;
        /// <summary>
        /// Description: Streams data in the same frequency as the initiated stream
        /// with the QTM server. If it manages to process packets faster that expected
        /// it will wait.
        /// </summary>
        protected void ContinuousStream()
        {
            long then = DateTime.Now.Ticks, now;

            while(streaming)
            {
                try
                {
                    if (!demoMode)
                    {
                        connection.Protocol.ReceiveRTPacket(out packetType);

                        // Make sure this is a data packet
                        if (packetType == PacketType.PacketData)
                        {
                            RetrieveDataAsync(connection.Protocol.GetRTPacket());
                        }
                    }
                    else
                    {
                        now = DateTime.Now.Ticks;

                        // Task.Delay causes some overhead and slows down
                        // execution a bit. This solution will work at different
                        // speeds depending on cpu. Find cross-platform way of 
                        // getting clock speed
                        if (now - then > frequency * 500)
                        {                            
                            // We don't need a packet for demo mode
                            RetrieveDataAsync(null);
                            then = now;
                        }

                        // Sleep thread to match desired frequency
                        //await Task.Delay(160 / frequency);                        
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

            if(!demoMode)
                connection.Dispose();
        }
    }
}

