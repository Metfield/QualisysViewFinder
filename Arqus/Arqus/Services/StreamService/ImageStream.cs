using Arqus.Helpers;
using ImageSharp;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.Services
{
    class ImageStream : Stream<ImageSharp.PixelFormats.Rgba32[]>
    {
        int limiter = 0;
        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency){ }

        private readonly object streamLock = new object();

        protected override void EnqueueDataAsync(RTPacket packet)
        {
            var data = packet.GetImageData();
            
            if(data.Count > 0)
            {
                foreach (var cameraImage in data)
                {
                    if (cameraImage.ImageData != null && cameraImage.ImageData.Length > 0)
                    {
                        if(limiter > 7)
                           return;

                        Task.Run( async () =>
                        {
                            lock(streamLock)
                            {
                                limiter++;
                            }
                            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            using (ImageSharp.Image imageData = await Task.Run(() => ImageSharp.Image.Load(cameraImage.ImageData)))
                            {
                                MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + cameraImage.CameraID, imageData.Pixels);
                                Thread.Sleep(50);
                                lock(streamLock)
                                {
                                    limiter--;
                                }
                            }
                        });
                    }
                }
            }
        }

        /*
        protected override bool GetCurrentFrame()
        {
            return QTMNetworkConnection.GetCurrentImageFrame();
        }
        */
    }
}
