using Arqus.Helpers;
using ImageSharp;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Services
{
    class ImageStream : Stream
    {

        public ImageStream(int frequency = 10) : base(ComponentType.ComponentImage, frequency){ }
        
        /// <summary>
        /// Name: GetImageData
        /// 
        /// Description: Decodes and return image data
        /// frome the current data packet of the stream
        /// 
        /// </summary>
        /// <returns>List of decoded images</returns>
        public List<Color[]> GetImageData()
        {
            List<Color[]> imageData = new List<Color[]>();
            
            if(currentPacket != null)
            {
                foreach (CameraImage camera in currentPacket?.GetImageData())
                {
                    imageData.Add(ImageProcessor.DecodeJPG(camera.ImageData));
                }

                return imageData;
            }
            else
            {
                return null;
            }

        }

        public Color[] GetImageData(int id)
        {
            if (currentPacket != null)
            {
                byte[] imageData = currentPacket.GetImageData(id).ImageData;
                return ImageProcessor.DecodeJPG(imageData);
            }

            return null;
        }

    }
}
