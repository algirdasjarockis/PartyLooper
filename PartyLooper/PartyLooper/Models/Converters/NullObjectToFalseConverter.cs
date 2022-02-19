using System;
using System.Globalization;
using Xamarin.Forms;

namespace PartyLooper.Models.Converters
{
    public class NullObjectToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
