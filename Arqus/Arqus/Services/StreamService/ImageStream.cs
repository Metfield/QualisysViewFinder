using Arqus.Helpers;
using ImageSharp;
using QTMRealTimeSDK.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Drawing;
using ImageSharp.Formats;
using Arqus.Service;
using System.Diagnostics;

namespace Arqus.Services
{
    class ImageStream : Stream<ImageSharp.PixelFormats.Rgba32[]>
    {
        int limiter = 0;
        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency){ }

        private readonly object streamLock = new object();

        private JpegDecoder decoder = new JpegDecoder();
        private long lastDecodeTimestamp;

        protected override void RetrieveDataAsync(RTPacket packet)
        {
            var data = packet.GetImageData();
            
            if(data.Count > 0)
            {
                foreach (var cameraImage in data)
                {
                    if (cameraImage.ImageData != null && cameraImage.ImageData.Length > 0)
                    {
                        if(limiter > 24)
                           return;

                        Task.Run(() =>
                        {
                            lock (streamLock)
                            {
                                limiter++;
                            }


                            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            ImageSharp.Image imageData = ImageSharp.Image.Load(cameraImage.ImageData, decoder);
                            if (timestamp > lastDecodeTimestamp)
                            {
                                lastDecodeTimestamp = timestamp;
                                MessagingService.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + cameraImage.CameraID, imageData.Pixels, track: false);
                            }

                            lock (streamLock)
                            {
                                limiter--;
                            }

                        });
                        
                    }
                }
            }
            //data.Clear();
        }
        
    }
}
