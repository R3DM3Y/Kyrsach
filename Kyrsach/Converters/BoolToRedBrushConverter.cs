using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Kyrsach.Converters
{
    public class BoolToRedBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isUrgent && isUrgent 
                ? new SolidColorBrush(Colors.LightPink) 
                : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}