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
        
        // Listens to events ;)
        private QTMEventListener qtmEventListener;

        public CameraStreamService() { }

        public void Start()
        {
            imageStream = new ImageStream();
            markerStream = new MarkerStream();

            imageStream.StartStream();

            markerStream.StartStream();
            qtmEventListener = new QTMEventListener(30, true);

            // Frequency of 30
            // Create event listener and start listening immediately
        }
        

        public void Dispose()
        {
            imageStream.Dispose();
            markerStream.Dispose();
            qtmEventListener.Dispose();
        }
    }
}

