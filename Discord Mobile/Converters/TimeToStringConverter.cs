using System;
using Windows.UI.Xaml.Data;

namespace Discord_Mobile.Converters
{
    class TimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string time = ((DateTimeOffset)value).LocalDateTime.ToString();
            return time.Substring(0,16);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
