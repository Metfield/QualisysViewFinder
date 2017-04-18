using System;
using System.Collections.Generic;
using System.Text;
using QTMRealTimeSDK.Data;
using Arqus.Services;
using System.Threading.Tasks;
using Xamarin.Forms;
using Arqus.Helpers;

namespace Arqus
{
    /// <summary>
    /// Name: CameraService
    /// 
    /// Description: This is meant to be an abstraction over the different streams
    /// for retrieving data in a in a camera centric way
    /// 
    /// </summary>
    public class CameraService : IDisposable
    {
        private bool running;
        private ImageStream imageStream;
        private MarkerStream markerStream;

        public CameraService() { }

        public void Start()
        {
            running = true;
            imageStream = new ImageStream();
            markerStream = new MarkerStream();

            imageStream.StartStream();
            markerStream.StartStream();

            // Make sure that the stream update loop runs in its own thread to keep interactions responsive
            Task.Run(() => UpdateDataTask(1, SendImageData));
            Task.Run(() => UpdateDataTask(30, SendMarkerData));
        }

        public Camera? GetMarkerData(int id)
        {
            return markerStream.GetMarkerData(id);
        }

        public ImageSharp.Color[] GetImageData(int id)
        {
            return imageStream.GetImageData(id);
        }

        /// <summary>
        /// Fetches new stream data and updates the local structure
        /// </summary>
        public async void UpdateDataTask(int frequency, Action onUpdate)
        {
            while (running)
            {
                DateTime time = DateTime.UtcNow;
                onUpdate();

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

        private async void SendImageData()
        {
            List<ImageSharp.Color[]> imageData = await Task.Run(() => imageStream.GetImageData());

            if (imageData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), imageData);
            }
        }

        private async void SendMarkerData()
        {
            List<QTMRealTimeSDK.Data.Camera> markerData = await Task.Run(() => markerStream.GetMarkerData());

            if (markerData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), markerData);
            }
        }

        public void Dispose()
        {
            running = false;
            imageStream.StopStream();
            markerStream.StopStream();
        }
    }
}
