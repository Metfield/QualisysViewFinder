using ImageSharp;
using ImageSharp.Formats;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Arqus.Helpers
{
    class ImageProcessor
    {
        public static ImageSharp.PixelFormats.Rgba32[] DecodeJPG(byte[] data)
        {
            DateTime time = DateTime.UtcNow;
            var img = Image.Load(data);
            Debug.WriteLine("Time to decode: " + (DateTime.UtcNow - time).TotalMilliseconds);
            return img.Pixels;
        }
    }
}
