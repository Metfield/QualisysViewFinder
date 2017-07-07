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
                    // Start stream
                    if (connection.Protocol.StreamFrames(StreamRate.RateFrequency, frequency, type))
                    {
                        // Disable other image-streaming cameras
                        if(type == ComponentType.ComponentImage)
                        {
                            for(int i = 1; i <= CameraStore.Cameras.Count; i++)
                            {
                                // Disable every camera but the current one
                                if(CameraStore.Cameras[i].Mode != CameraMode.ModeMarker && CameraStore.Cameras[i].ID != CameraStore.CurrentCamera.ID)
                                {
                                    SettingsService.DisableImageMode(i);
                                }
                            }
                        }

                        // Run continuous stream method
                        streamTask = Task.Run(() => ContinuousStream());
                    }
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
        protected async void ContinuousStream()
        {
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
                            // IMPORTANT: Video stream needs to run as an asynchronous task in 
                            // order to work properly
                            Task.Run(() => RetrieveDataAsync(connection.Protocol.GetRTPacket()));
                        }
                    }
                    else
                    {
                        // We don't need a packet for demo mode
                        // IMPORTANT: Demo mode will crash application after ~10 seconds
                        // if RetreiveDataAsync is not called on the main thread
                        Urho.Application.InvokeOnMainAsync(() => RetrieveDataAsync(null));

                        // Sleep thread to match desired frequency
                        // NOTE: Task.Delay was causing some overhead and slowed down
                        // the execution quite noticeably. It seems to be working properly
                        // now, but if for some demo stream becomes slow again. CHECK HERE!
                        await Task.Delay(160 / frequency);
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

