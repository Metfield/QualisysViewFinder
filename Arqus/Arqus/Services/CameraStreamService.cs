using System;
using Arqus.Services;
using Acr.UserDialogs;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Arqus
{
    /// <summary>
    /// Name: CameraService
    /// 
    /// Description: This is meant to be an abstraction over the different streams
    /// for retrieving data in a in a camera centric way
    /// 
    /// </summary>
    public class CameraStreamService : IDisposable
    {
        private ImageStream imageStream;
        private MarkerStream markerStream;

        private DemoStream demoStream;
        bool streamingDemo;

        // Listens to events ;)
        private QTMEventListener qtmEventListener;

        public CameraStreamService(bool demoMode = false)
        {
            streamingDemo = demoMode;
        }

        bool shouldStartMeasurement;

        public async void Start()
        {
            if (!streamingDemo)
            {
                imageStream = new ImageStream();
                markerStream = new MarkerStream();

                // If this is false it means that there is a QTM instance without any 
                if (!(imageStream.StartStream() && markerStream.StartStream()))
                {
                    if (!QTMNetworkConnection.IsMaster)
                    {
                        await UserDialogs.Instance.AlertAsync("There is no active measurement. Connect to the system in master mode to be able to start a new one." +
                                                              " Alternatively, start a measurement directly from QTM", "Attention");
                    }
                    else if (await UserDialogs.Instance.ConfirmAsync("There is no active measurement, would you like to start one?", null, "Start Measurement", "Cancel"))
                    {
                        SettingsService.StartMeasurement();

                        // Wait a maximum of three seconds for data to start coming
                        int iterationsToWait = 30;

                        // Attempt to restart streams
                        while (!(markerStream.StartStream() && imageStream.StartStream()) && iterationsToWait > 0)
                        {
                            iterationsToWait--;
                            System.Threading.Thread.Sleep(100);

                            // This is an unlikely scenario
                            if (iterationsToWait == 0)
                                Debugger.Break();
                        }

                        // Re-select current camera to start streaming again
                        CameraStore.SetCurrentCamera(CameraStore.CurrentCamera.ID);
                    }
                }

                // Frequency of 30
                // Create event listener and start listening immediately
                qtmEventListener = new QTMEventListener(30, true);
            }
            else
            {               
                demoStream = new DemoStream();
                demoStream.StartStream();
            }
        }

        // Updates every image-enabled camera in system
        // NOTE: Used when in grid mode
        public void UpdateGridCameras()
        {
            imageStream.UpdateGridCameras();
        }

        // Prepares the stream for grid mode streaming by temporarily pausing
        // the normal image stream
        public void StartGridCamerasUpdate()
        {
            imageStream.PauseDetailStream();
        }

        // Resume normal image streaming
        public void StopGridCamerasUpdate()
        {
            imageStream.ResumeDetailStream();
        }

        public void Dispose()
        {
            if (!streamingDemo)
            {
                imageStream.Dispose();
                markerStream.Dispose();
                qtmEventListener.Dispose();
            }
            else
            {
                demoStream.Dispose();
                demoStream.Clean();
            }

            streamingDemo = false;
        }
    }
}



