using Arqus.Helpers;
using ImageSharp;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public List<CameraImage> GetImageData()
        {
            List<Color[]> imageData = new List<Color[]>();
            
            if(currentPacket != null)
            {
                return currentPacket.GetImageData();
            }
            else
            {
                return null;
            }
        }

        public async Task<Color[]> GetImageData(int id)
        {
            if (currentPacket != null)
            {
                byte[] imageData = currentPacket.GetImageData(id).ImageData;
                return await ImageProcessor.DecodeJPG(imageData);
            }

            return null;
        }
    }
}
