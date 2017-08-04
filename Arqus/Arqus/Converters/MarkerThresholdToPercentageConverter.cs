using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Arqus.Converters
{
    class MarkerThresholdToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                float markerThreshold = float.Parse(value.ToString());
                float markerThresholdInPercentage = markerThreshold / 10;

                return String.Format("{0}%", (int)markerThresholdInPercentage);
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine("MarkerThresholdToPercentageConverter:", e.Message);
            }

            return "n/a";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
