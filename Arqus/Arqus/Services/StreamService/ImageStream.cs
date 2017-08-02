using Arqus.Helpers;

using QTMRealTimeSDK.Data;
using System;
using System.Diagnostics;

using System.Collections.Generic;

using SkiaSharp;

namespace Arqus.Services
{
    class ImageStream : Stream
    {
        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency, false){ }
        
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

                    // Load and decode image information, then resize it to a squared, power of 2 size
                    SKBitmap bitmap = SKBitmap.Decode(data[0].ImageData).Resize(new SKImageInfo(
                        Constants.URHO_TEXTURE_SIZE, Constants.URHO_TEXTURE_SIZE),
                        SKBitmapResizeMethod.Lanczos3);

                isDecoding = false;

                // Set current camera's image data and ready it
                // to create a texture
                if (CameraStore.CurrentCamera.Screen != null)
                    CameraStore.CurrentCamera.Screen.ImageData = bitmap.Bytes;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debugger.Break();
            }
        }        
    }
}
