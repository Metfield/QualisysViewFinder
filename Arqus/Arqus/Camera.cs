using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Arqus.Camera2D
{
    public class ImageResolution
    {
        public int Width { private set; get; }
        public int Height { private set; get; }

        public ImageResolution(int width, int height)
        {
           
            Width = width;
            Height = height;

            Debug.WriteLine("Widht " + Width.ToString());
            Debug.WriteLine("Height " + Height.ToString());
        }

        public float PixelAspectRatio
        {
            get
            {
                return (float) Width / (float) Height;
            }
        }
    }
}
