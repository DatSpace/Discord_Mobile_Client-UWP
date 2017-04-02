using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Discord_Mobile.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush color;
            switch (value)
            {
                case UserStatus.Online:
                    color = new SolidColorBrush(Colors.SpringGreen);
                    break;
                case UserStatus.Idle:
                    color = new SolidColorBrush(Colors.Orange);
                    break;
                case UserStatus.DoNotDisturb:
                    color = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    color = new SolidColorBrush(Colors.Black);
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class UserToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Discord.Color userColor = new Discord.Color(255,255,255);

            if (value.GetType() == typeof(SocketGuildUser))
            {
                userColor = ((SocketGuildUser)value).Roles.OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? userColor;
            }
            else if (value.GetType() != typeof(RestUser) && value.GetType() != typeof(RestWebhookUser))
            {
                userColor = ((IEnumerable<SocketRole>)value).OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? userColor;
            }

            SolidColorBrush color = new SolidColorBrush(Windows.UI.Color.FromArgb(255,userColor.R, userColor.G, userColor.B));
            
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthorToAvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage image = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            if (value != null)
            {
                string url;
                if (value.GetType() == typeof(SocketGuildUser))
                    url = ((SocketGuildUser)value).GetAvatarUrl();
                else
                    url = ((RestUser)value).GetAvatarUrl();

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

    public class VoiceUsersToCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && parameter != null)
            {
                int online = ((IReadOnlyCollection<SocketGuildUser>)(value)).Count;
                return string.Format("(" + online + "/" + parameter + ")");
            }
            else
                return "Ukn";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
