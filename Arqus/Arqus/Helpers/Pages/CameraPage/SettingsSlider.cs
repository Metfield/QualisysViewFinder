using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public class SettingsSlider
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Value { get; set; }

        public SettingsSlider(double min, double max, double value)
        {
            Max = max;
            Min = min;
            Value = value;
        }
    }
}
