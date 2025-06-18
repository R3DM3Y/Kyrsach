using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Kyrsach
{
    public class DiscountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasDiscount)
                return hasDiscount;
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}