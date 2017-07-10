﻿using Arqus.Helpers;
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
        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency, false){ }
        
        // Re-use Jpeg decoder for every frame
        private JpegDecoder decoder = new JpegDecoder();

        // Used for task-workload leveling
        static bool isDecoding = false;

        protected override void RetrieveDataAsync()
        {
            // TODO: Implement video buffer instead
            // Don't pull another frame if previous one has not yet been consumed
            if (isDecoding)
                return;

            // Get image data if another process has finished decoding
            RTPacket packet = connection.Protocol.GetRTPacket();
            List<CameraImage> data = packet.GetImageData();

            // Return if there is no data (rare)
            if (data.Count == 0)
                return;

            try
            {
                // Decode and load image
                isDecoding = true;
                    Image<Rgba32> imageData = ImageSharp.Image.Load(data[0].ImageData, decoder);
                isDecoding = false;

                // Set current camera's image data and ready it 
                // to create a texture
                if (CameraStore.CurrentCamera.Screen != null)
                    CameraStore.CurrentCamera.Screen.ImageData = imageData;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debugger.Break();
            }
        }        
    }
}
