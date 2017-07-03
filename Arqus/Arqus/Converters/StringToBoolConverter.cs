using System;
using System.Globalization;
using Xamarin.Forms;
using Arqus.Helpers;

namespace Arqus.Converters
{
    // Used by the buttons on the mode toolbar
    public class StringToBoolConverter : IValueConverter
    {
        // Source to view
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Switch is neat!
            switch (parameter.ToString())
            {
                case "Video":

                    if ((bool)value)
                        return Constants.MODEBAR_ICON_VIDEO_DEMO;
                    else
                        return Constants.MODEBAR_ICON_VIDEO_NORMAL;

                case "Intensity":

                    if ((bool)value)
                        return Constants.MODEBAR_ICON_INTENSITY_DEMO;
                    else
                        return Constants.MODEBAR_ICON_INTENSITY_NORMAL;
            }

            return value;
        }

        // View to source
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We don't need this
            return value;
        }
    }
}
