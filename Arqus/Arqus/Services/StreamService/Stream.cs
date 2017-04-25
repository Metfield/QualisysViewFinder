using Arqus.Helpers;
using Priority_Queue;
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
    /// 
    /// Name: StreamService
    /// Created: 2017-04-05
    /// 
    /// Description: It is meant to retrieve RT packets in a 
    /// consistent way as well as enable developers
    /// to retrieve the packet of interest in an
    /// intutive manner
    /// 
    /// </summary>
    abstract class Stream<TData> : IDisposable
    {

        protected Dictionary<uint, SimplePriorityQueue<TData, long>> dataQueue;

        private ComponentType type;
        protected bool streaming;
        protected int frequency;

        // Variables to handle packets
        protected QTMNetworkConnection connection;

        protected int port;
        static int streamCount;

        protected Stream(ComponentType type, int frequency)
        {
            this.type = type;
            this.frequency = frequency;
            connection = new QTMNetworkConnection();
            dataQueue = new Dictionary<uint, SimplePriorityQueue<TData, long>>();
            port = streamCount + 2230;
            streamCount++;
        }

        public void StartStream()
        {
            if (!streaming)
            {
                streaming = true;

                // NOTE: We might have to initatiate a unique network instance for each stream
                bool success = connection.Protocol.StreamFrames(StreamRate.RateFrequency, frequency, type);
                //bool success = true;


                if (!success)
                    return;
                else
                    // Run stream in its own thread to prevent it from blocking more time critical processes on the main thread
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

        /// <summary>
        /// Description: Streams data in the same frequency as the initiated stream
        /// with the QTM server. If it manages to process packets faster that expected
        /// it will wait.
        /// </summary>
        protected async void ContinuousStream()
        {
            while (streaming)
            {
                DateTime time = DateTime.UtcNow;

                PacketType packetType = new PacketType();
                connection.Protocol.ReceiveRTPacket(out packetType);
                RTPacket packet = connection.Protocol.GetRTPacket();

                if (packet != null)
                    EnqueueDataAsync(packet);

                if (dataQueue.Count > 0)
                    //SendData();
                
                if (frequency > 0)
                {
                    var deltaTime = (DateTime.UtcNow - time).TotalMilliseconds;
                    var timeToWait = 1000d / frequency - deltaTime;

                    if (timeToWait >= 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(timeToWait));
                    }
                }

            }
        }

        protected abstract void EnqueueDataAsync(RTPacket packet);

       
        protected void SendData()
        {
            foreach(var id in dataQueue.Keys)
            {
                try
                {
                    if(dataQueue[id].Count > 0)
                    {
                        TData data = dataQueue[id].Dequeue();
                        MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + id, data);
                    }
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public void Dispose()
        {
            StopStream();
            connection.Protocol.Dispose();
        }

        protected void Enqueue(Dictionary<uint, SimplePriorityQueue<TData, long>> dataQueue, uint id, TData data, long timestamp)
        {
            if (!dataQueue.ContainsKey(id))
                dataQueue.Add(id, new SimplePriorityQueue<TData, long>());

            dataQueue[id].Enqueue(data, timestamp);
        }
    }
}
