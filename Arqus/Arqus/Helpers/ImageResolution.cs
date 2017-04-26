using System.Diagnostics;

namespace Arqus.Helpers
{
    public class ImageResolution : Resolution
    {

        public ImageResolution(int width, int height) : base(width, height) { }

        public float PixelAspectRatio
        {
            get
            {
                return Width / (float) Height;
            }
        }

        public int PixelCount
        {
            get
            {
                return Width * Height;
            }
        }
    }  

}
