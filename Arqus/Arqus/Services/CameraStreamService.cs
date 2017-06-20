using System;
using Arqus.Services;

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

        // this is been CALLED TWICE AFTER GOING BACK ONCE FROM DEMO MODE
        // START GETS CALLED BEFORE THE CONSTRUCTOR
        public void Start()
        {
            if (!streamingDemo)
            {
                imageStream = new ImageStream();
                markerStream = new MarkerStream();

                imageStream.StartStream();
                markerStream.StartStream();

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
            }

            streamingDemo = false;
        }
    }
}



