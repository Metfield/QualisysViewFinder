using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Arqus.Converters
{
    class PrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float fvalue = float.Parse(value.ToString());

            if(parameter.ToString() == "m" || parameter.ToString() == "f")
                return String.Format("{0} {1}", fvalue.ToString("0.00"), parameter);
            else
                return String.Format("{0} {1}", (int)fvalue, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
