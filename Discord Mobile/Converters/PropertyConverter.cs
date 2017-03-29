using Discord;
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
            Discord.Color userColor;

            if (value.GetType() == typeof(SocketGuildUser))
            {
                userColor = ((SocketGuildUser)value).Roles.OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? Discord.Color.Default;
            }
            else
            {
                userColor = ((IEnumerable<SocketRole>)value).OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? Discord.Color.Default;
            }

            uint red = userColor.R;
            uint green = userColor.G;
            uint blue = userColor.B;

            if (userColor.RawValue != Discord.Color.Default.RawValue)
            {
                red = userColor.R;
                green = userColor.G;
                blue = userColor.B;
            }
            else
            {
                red = 255;
                green = 255;
                blue = 255;
            }


            SolidColorBrush color = new SolidColorBrush(Windows.UI.Color.FromArgb(255,(byte)red, (byte)green, (byte)blue));
            
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
            BitmapImage image;
            if (value == null)
                image = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                image = new BitmapImage(new Uri(((SocketGuildUser)value).GetAvatarUrl()));
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
