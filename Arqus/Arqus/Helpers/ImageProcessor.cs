using ImageSharp;
using ImageSharp.Formats;

namespace Arqus.Helpers
{
    class ImageProcessor
    {
        public static Color[] DecodeJPG(byte[] data)
        {
            return Image.Load(data).Pixels;
        }
    }
}
