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
using Arqus.Visualization;
using System.Collections.Generic;
using Priority_Queue;

namespace Arqus.Services
{
    class ImageStream : Stream<ImageSharp.PixelFormats.RgbaVector>
    {
        int limiter = 0;
        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency, false){ }
        
        // Re-use Jpeg decoder for every frame
        private JpegDecoder decoder = new JpegDecoder();
        private SimplePriorityQueue<Image<Rgba32>> queue = new SimplePriorityQueue<Image<Rgba32>>();
        

        protected override void RetrieveDataAsync(RTPacket packet)
        {

            // Get image data if another process has finished decoding
            List<CameraImage> data = packet.GetImageData();

            if (data.Count == 0)
                return;

            try
            {
                // Decode and load image
                // NOTE: This is just an arbitrary value to determine how many images that should
                // be decoded at the most
                if(queue.Count < 4)
                   queue.Enqueue(ImageSharp.Image.Load(data[0].ImageData, decoder), DateTimeOffset.Now.ToUnixTimeMilliseconds());

                // Set current camera's image data and ready it 
                // to create a texture
                if (CameraStore.CurrentCamera.Screen != null && queue.Count > 0)
                    CameraStore.CurrentCamera.Screen.ImageData = queue.Dequeue();              
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debugger.Break();
            }
        }        
    }
}
