using ImageSharp;
using ImageSharp.Formats;
using System.Threading.Tasks;

namespace Arqus.Helpers
{
    class ImageProcessor
    {
        public static async Task<Color[]> DecodeJPG(byte[] data)
        {
            return await Task.Run(() => Image.Load(data).Pixels);
        }
    }
}
