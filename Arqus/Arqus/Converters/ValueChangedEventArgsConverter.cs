using System;
using System.Globalization;
using Xamarin.Forms;

namespace Arqus.Converters
{
    class ValueChangedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueChangedEventArgs = value as ValueChangedEventArgs;
            if(valueChangedEventArgs == null)
            {
                throw new ArgumentException("Expected value to be of type ValueChangedEventArgs", nameof(value));
            }
            return valueChangedEventArgs;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
