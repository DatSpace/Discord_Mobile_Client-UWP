using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Discord;
using Discord.WebSocket;
using Discord.Rest;

namespace Discord_Mobile.Converters
{
    public class UserToAvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage image = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            if (value != null)
            {
                string url;
                if (value is SocketGuildUser)
                    url = ((SocketGuildUser)value).GetAvatarUrl();
                else if (value is RestUser)
                    url = ((RestUser)value).GetAvatarUrl();
                else if (value is RestWebhookUser)
                    url = ((RestWebhookUser)value).GetAvatarUrl();
                else if (value is SocketUser)
                    url = ((SocketUser)value).GetAvatarUrl();
                else
                    url = ((SocketSelfUser)value).GetAvatarUrl();

                if (url != null)
                    image = new BitmapImage(new Uri(url));
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class GuildToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s;

            if (((SocketGuild)value).IconUrl == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(((SocketGuild)value).IconUrl));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
