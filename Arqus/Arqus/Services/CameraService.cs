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
    public class CameraService : ICameraService
    {
        private ImageStream imageStream;
        private MarkerStream markerStream;

        public CameraService()
        {
            imageStream = new ImageStream();
            imageStream.StartStream();

            markerStream = new MarkerStream();
            markerStream.StartStream();

                        // Make sure that the stream update loop runs in its own thread to keep interactions responsive
            Task.Run(() => UpdateDataTask(1, () =>   ) );
            Task.Run(() => UpdateDataTask(30, () =>  ) );


            // Set up messaging system for stream handling.
            // This is not optimal since it demands a lot
            // of messaging do render an image sequence
            // but for now it is to my knowledge the more
            // portable way of doing it.
            MessagingCenter.Subscribe<Urho.Application>(this,
                MessageSubject.FETCH_IMAGE_DATA.ToString(),
                OnFetchImageData);

            MessagingCenter.Subscribe<Urho.Application>(this,
                MessageSubject.FETCH_MARKER_DATA.ToString(),
                OnFetchMarkerData);
        }

        ~CameraService()
        {
            imageStream.StopStream();
            markerStream.StopStream();
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
            while (true)
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



        private async void OnFetchImageData(Object sender)
        {
            List<ImageSharp.Color[]> imageData = await Task.Run(() => imageStream.GetImageData());

            if (imageData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), imageData);
            }
        }

        private async void OnFetchMarkerData(Object sender)
        {
            List<QTMRealTimeSDK.Data.Camera> markerData = await Task.Run(() => markerStream.GetMarkerData());

            if (markerData != null)
            {
                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), markerData);
            }
        }


    }
}
