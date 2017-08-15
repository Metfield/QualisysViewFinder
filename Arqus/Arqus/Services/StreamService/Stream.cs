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
    abstract class Stream : IDisposable
    {
        private ComponentType type;
        protected bool streaming;
        protected int frequency;

        private bool demoMode;

        // Variables to handle packets
        protected QTMNetworkConnection connection;

        protected Stream(ComponentType type, int frequency, bool demoMode)
        {
            this.type = type;
            this.frequency = frequency;
            this.demoMode = demoMode;

            if(!demoMode)
                connection = new QTMNetworkConnection();
        }

        public bool StartStream()
        {
            if (!streaming)
            {
                streaming = true;

                if (!demoMode)
                {
                    // Start stream
                    if (connection.Protocol.StreamFrames(StreamRate.RateFrequency, frequency, type))
                    {
                        // Return false if there is no active measurement
                        if (!connection.IsMeasurementActive())
                        {
                            streaming = false;
                            return false;
                        }

                        // Disable other image-streaming cameras
                        if (type == ComponentType.ComponentImage)
                        {
                            for(int i = 1; i <= CameraManager.Cameras.Count; i++)
                            {
                                // Disable every camera but the current one
                                if(CameraManager.Cameras[i].Settings.Mode != QTMRealTimeSDK.Settings.CameraMode.ModeMarker && CameraManager.Cameras[i].ID != CameraManager.CurrentCamera.ID)
                                {
                                    SettingsService.DisableImageMode(i);
                                }
                            }
                        }
                    }
                }

                // Run continuous stream method
                // TODO: In case of demo mode, this assumes it all went well
                Task.Factory.StartNew(() => ContinuousStream(), TaskCreationOptions.LongRunning);
            }
            else
            {
                Debug.WriteLine("Already streaming: ", type.ToString());
            }

            return true;
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
                            
                            Task.Run(() => RetrieveDataAsync());
                        }
                    }
                    else
                    {
                        // We don't need a packet for demo mode
                        // IMPORTANT: Demo mode will crash application after ~10 seconds
                        // if RetreiveDataAsync is not called on the main thread
                        Urho.Application.InvokeOnMainAsync(() => RetrieveDataAsync());

                        // Sleep thread to match desired frequency
                        // NOTE: Task.Delay was causing some overhead and slowed down
                        // the execution quite noticeably. It seems to be working properly
                        // now, but if for some demo stream becomes slow again. CHECK HERE!
                        await Task.Delay(160 / frequency);
                    }
                }
                catch (Exception e)
                {
                    GC.Collect();

                    Debug.WriteLine(e);
                    Debugger.Break();
                }                
            }
        }

        protected abstract void RetrieveDataAsync();
        
        public void Dispose()
        {
            StopStream();

            if(!demoMode)
                connection.Dispose();
        }
    }
}

