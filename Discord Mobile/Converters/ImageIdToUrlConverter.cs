using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Discord_Mobile.Converters
{
    public class AvatarIdToUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s;
            if (value == null || parameter == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(String.Format("https://cdn.discordapp.com/avatars/" + parameter.ToString() + "/" + value.ToString() + ".png?size=128")));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IconIdToUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s;
            if (value == null || parameter == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(String.Format("https://cdn.discordapp.com/icons/" + parameter.ToString() + "/" + value.ToString() + ".png?size=128")));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
