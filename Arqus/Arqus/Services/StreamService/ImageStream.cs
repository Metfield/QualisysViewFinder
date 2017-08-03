using Arqus.Helpers;

using QTMRealTimeSDK.Data;
using System;
using System.Diagnostics;

using System.Collections.Generic;

using SkiaSharp;
using System.Threading.Tasks;

namespace Arqus.Services
{
    class ImageStream : Stream
    {
        private bool isUpdatingGridCameras = false;

        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency, false){ }
        
        // Used for task-workload leveling
        static bool isDecoding = false;

        protected override void RetrieveDataAsync()
        {
            // TODO: Implement video buffer instead
            // Don't pull another frame if previous one has not yet been consumed
            // ------------------------------------------------------------------
            // Don't stream anything if grid layout is enabled
            if (isDecoding || isUpdatingGridCameras)
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
                if (CameraStore.CurrentCamera.Screen != null && data[0].CameraID == CameraStore.CurrentCamera.ID)
                    CameraStore.CurrentCamera.Screen.ImageData = bitmap.Bytes;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debugger.Break();
            }
        }

        // Update every image-enabled camera in the system
        public void UpdateGridCameras()
        {
            DataModels.Camera camera;
            List<CameraImage> cameraImages = connection.Protocol.GetRTPacket().GetImageData();

            for (int i = 0; i < CameraStore.Cameras.Count; i++)
            {
                camera = CameraStore.Cameras[i + 1];

                if (!camera.IsImageMode())
                    continue;
                
                Parallel.For(0, cameraImages.Count, (index) => ParallelUpdate(index, cameraImages[index], camera));
            }
        }

        private void ParallelUpdate(int index, CameraImage cameraImage, DataModels.Camera localCamera)
        {
            if (cameraImage.CameraID != localCamera.ID)
                return;

            SKBitmap bitmap = SKBitmap.Decode(cameraImage.ImageData).Resize(new SKImageInfo(
                Constants.URHO_TEXTURE_SIZE, Constants.URHO_TEXTURE_SIZE),
                SKBitmapResizeMethod.Lanczos3);

            if (localCamera.Screen != null)
            {
                localCamera.Screen.ImageData = bitmap.Bytes;
            }
        }

        // Pauses normal image streaming
        public void PauseDetailStream()
        {
            isUpdatingGridCameras = true;
        }

        // Resumes normal image streaming
        public void ResumeDetailStream()
        {
            isUpdatingGridCameras = false;
        }
    }
}
