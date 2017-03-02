using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Discord_Mobile.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object res;
            res = (value == null ? false : true) ? string.IsNullOrEmpty(value.ToString()) ? null : new BitmapImage(new Uri(value.ToString())) : null;
            return res;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
