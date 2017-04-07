﻿using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

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
    abstract class Stream
    {
        private bool streaming;
        private ComponentType type;
        private int frequency;

        // Variables to handle packets
        private PacketType packetType;
        protected RTPacket currentPacket;

        // We need a lock object to prevent a packet from getting overwritten
        // during retrieval
        private Object thisLock;

        protected Stream(ComponentType type, int frequency)
        {
            this.type = type;
            this.frequency = frequency;
        }

        public void StartStream()
        {
            if (!streaming)
            {
                streaming = true;

                // NOTE: We might have to initatiate a unique network instance for each stream
                bool success = QTMNetworkConnection.Instance.protocol.StreamFrames(StreamRate.RateFrequency, frequency, type);

                if (!success)
                    return;
                else
                    // Run stream in its own thread to prevent it from blocking more time critical processes on the main thread
                    Task.Run( () => ContinuousStream());

            }
            else
            {
                Debug.WriteLine("Already streaming: ", type.ToString());
            }
        }


        public void StopStream()
        {
            streaming = false;
            currentPacket = null;
        }

        /// <summary>
        /// Name: Stream
        /// Created: 2017-04-05
        /// 
        /// Description: Streams data in the same frequency as the initiated stream
        /// with the QTM server. If it manages to process packets faster that expected
        /// it will wait.
        /// </summary>
        private async void ContinuousStream()
        {
            while (streaming)
            {
                DateTime time = DateTime.UtcNow;
                QTMNetworkConnection.Instance.protocol.ReceiveRTPacket(out packetType);
            
                currentPacket = QTMNetworkConnection.Instance.protocol.GetRTPacket();

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

    }
}
