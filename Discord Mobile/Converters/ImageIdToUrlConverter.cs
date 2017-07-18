﻿using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Discord;
using Discord.WebSocket;

namespace Discord_Mobile.Converters
{
    public class UserToAvatarUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            string url = ((SocketGuildUser)value).GetAvatarUrl();
            if (url != null)
                s = new BitmapImage(new Uri(url));

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
            if (value == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(value.ToString()));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class RecipientToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage s;
            if (value == null)
                s = new BitmapImage(new Uri("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png"));
            else
                s = new BitmapImage(new Uri(((IUser)value).GetAvatarUrl()));
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
