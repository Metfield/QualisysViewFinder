using System;
using System.Collections.Generic;
using System.Text;
using QTMRealTimeSDK.Data;
using Arqus.Services;
using System.Threading.Tasks;
using Xamarin.Forms;
using Arqus.Helpers;
using Arqus.Visualization;
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
        private bool running;
        private ImageStream imageStream;
        private MarkerStream markerStream;

        public CameraStreamService() { }

        public void Start()
        {
            running = true;
            imageStream = new ImageStream();
            markerStream = new MarkerStream();

            imageStream.StartStream();
            markerStream.StartStream();

            // Make sure that the stream update loop runs in its own thread to keep interactions responsive
            Task.Run(() => UpdateDataTask(10, SendImageData));
            Task.Run(() => UpdateDataTask(30, SendMarkerData));
        }

        public Camera? GetMarkerData(int id)
        {
            return markerStream.GetMarkerData(id);
        }

        public async Task<ImageSharp.Color[]> GetImageData(int id)
        {
            return await imageStream.GetImageData(id);
        }

        /// <summary>
        /// Fetches new stream data and updates the local structure
        /// </summary>
        public async void UpdateDataTask(int frequency, Func<Task<bool>> onUpdate)
        {
            while (running)
            {
                DateTime time = DateTime.UtcNow;
                await onUpdate();

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

        private async Task<bool> SendImageData()
        {
            List<CameraImage> cameraImages = await Task.Run(() => imageStream.GetImageData());

            if (cameraImages != null)
            {
                foreach(CameraImage cameraImage in cameraImages)
                {
                    try
                    {
                        ImageSharp.Color[] imageData = await ImageProcessor.DecodeJPG(cameraImage.ImageData);

                        if (imageData != null)
                        {
                            MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + cameraImage.CameraID, imageData);
                            return true;
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            return false;
        }

        private async Task<bool> SendMarkerData()
        {
            List<QTMRealTimeSDK.Data.Camera> cameras = await Task.Run(() => markerStream.GetMarkerData());

            if (cameras != null)
            {
                // Camera IDs start at 1 in QTM.
                // Furthermore the list will be ordered according to the ID,
                // so looping through the list and incrementing the ID every
                // time will reflect the camera state in QTM as well.
                for (int cameraID = 1; cameraID <= cameras.Count; cameraID++)
                {
                    MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + cameraID, cameras[cameraID - 1]);
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            running = false;
            imageStream.Dispose();
            markerStream.Dispose();
        }
    }
}
