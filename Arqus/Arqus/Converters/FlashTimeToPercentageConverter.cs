using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Arqus.Converters
{
    class FlashTimeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int flashtime = (int)value;
                float flashtimeInPercentage = flashtime / 10;

                return String.Format("{0}%", flashtimeInPercentage);
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine("FlashTimeToPercentageConverter:", e.Message);
            }

            return "n/a";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
