using ImageSharp;
using ImageSharp.Formats;
using System.Threading.Tasks;

namespace Arqus.Helpers
{
    class ImageProcessor
    {
        public static async Task<Color[]> DecodeJPG(byte[] data)
        {
            var img = await Task.Run(() => Image.Load(data));
            return img.Pixels;
        }
    }
}
