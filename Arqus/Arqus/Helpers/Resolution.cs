using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public class Resolution
    {
            public int Width { protected set; get; }
            public int Height { protected set; get; }

            public Resolution(int width, int height) { Width = width; Height = height; }
    }
}
