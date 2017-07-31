using System;
using System.Globalization;
using Xamarin.Forms;

namespace Arqus.Converters
{
    class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() ==  parameter.ToString())
                return "#303030";

            return "#212121";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return parameter;

            //it's false, so don't bind it back
            throw new Exception("EqualityToBooleanConverter: It's false, I won't bind back.");
        }
    }
}
