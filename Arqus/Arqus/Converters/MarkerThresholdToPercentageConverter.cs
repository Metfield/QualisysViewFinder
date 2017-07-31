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
                int markerThreshold = (int)value;
                float markerThresholdInPercentage = markerThreshold / 10;

                return String.Format("{0}%", markerThresholdInPercentage);
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine("MarkerThresholdToPercentageConverter:", e.Message);
            }

            return "n/a";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
