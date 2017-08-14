using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    class Slider
    {
        public double Max { get; set; }
        public double Min { get; set; }

        public Slider(double max, double min)
        {
            Max = max;
            Min = min;
        }
    }
}
