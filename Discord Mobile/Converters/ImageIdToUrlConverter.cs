using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Discord_Mobile.ViewModels;

namespace Discord_Mobile.Converters
{
    public class AvatarIdToUrlConverter : IValueConverter //Change name
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            foreach (var user in ChatViewModel.GuildUserList)
            {
                if (user.Id == (ulong)value && user.GetAvatarUrl() != null)
                {
                    s = new BitmapImage(new Uri(user.GetAvatarUrl()));//(String.Format("https://cdn.discordapp.com/avatars/" + parameter.ToString() + "/" + value.ToString() + ".png?size=128")));
                    break;
                }
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class UrlToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s;
            if (value == null) //|| parameter == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(value.ToString()));//(String.Format("https://cdn.discordapp.com/icons/" + parameter.ToString() + "/" + value.ToString() + ".png?size=128")));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
