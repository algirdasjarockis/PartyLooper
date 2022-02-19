using System;
using System.Globalization;
using Xamarin.Forms;

namespace PartyLooper.Models.Converters
{
    public class EmptyStringToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (string.Empty != (value as string));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
