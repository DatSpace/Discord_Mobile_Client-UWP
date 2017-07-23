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
using Discord_Mobile.ViewModels;
using Windows.UI.Xaml;
using Discord_Mobile.Services;

namespace Discord_Mobile.Converters
{
    public class UserToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //if (((SocketGuildUser)value).Id == LoginService.client.CurrentUser.Id)
            //{
            //    return new SolidColorBrush
            //}
            switch (((IUser)value).Status)
            {
                case UserStatus.Online:
                    return new SolidColorBrush(Colors.MediumSpringGreen);
                case UserStatus.Idle:
                case UserStatus.AFK:
                    return new SolidColorBrush(Colors.DarkOrange);
                case UserStatus.DoNotDisturb:
                    return new SolidColorBrush(Colors.Red);
                case UserStatus.Offline:
                default:
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (((IMessage)value).EditedTimestamp.HasValue)
                return (((IMessage)value).EditedTimestamp.Value.LocalDateTime.ToString()).Substring(0, 16) + " (Edited)";
            else
                return ((IMessage)value).Timestamp.LocalDateTime.ToString().Substring(0, 16);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameExtentionToFileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage image = null;

            int lastIndex = ((string)value).LastIndexOf(".");
            string fileExtention = ((string)value).Substring(lastIndex);

            switch (fileExtention)
            {
                case ".pdf":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/pdf-24.ico"));
                    break;
                case ".csv":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/csv-24.ico"));
                    break;
                case ".txt":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/document-24.ico"));
                    break;
                case ".xls":
                case ".xlt":
                case ".xlm":
                case ".xlsx":
                case ".xlsm":
                case ".xltx":
                case ".xltm":
                case ".xlsb":
                case ".xla":
                case ".xlam":
                case ".xll":
                case ".xlw":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/exel-24.ico"));
                    break;
                case ".ppt":
                case ".pot":
                case ".pps":
                case ".pptx":
                case ".pptm":
                case ".potx":
                case ".potm":
                case ".ppam":
                case ".ppsx":
                case ".ppsm":
                case ".sldx":
                case ".sldm":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/powerpoint-3-24.ico"));
                    break;
                case ".doc":
                case ".dot":
                case ".wbk":
                case ".docx":
                case ".docm":
                case ".dotx":
                case ".dotm":
                case ".docb":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/word-3-24.ico"));
                    break;
                case ".avi":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/avi-24.ico"));
                    break;
                case ".flv":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/flv-24.ico"));
                    break;
                case ".mov":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/mov-24.ico"));
                    break;
                case ".mpg":
                case ".mpeg":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/mpg-24.ico"));
                    break;
                case ".dll":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/dll-24.ico"));
                    break;
                case ".exe":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/exe-24.ico"));
                    break;
                case ".psd":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/psd-24.ico"));
                    break;
                case ".gif":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/gif-24.ico"));
                    break;
                case ".jpg":
                case ".jpeg":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/jpg-24.ico"));
                    break;
                case ".png":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/png-24.ico"));
                    break;
                case ".rar":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/rar-24.ico"));
                    break;
                case ".zip":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/zip-24.ico"));
                    break;
                case ".mp3":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/mp3-24.ico"));
                    break;
                case ".wma":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/wma-24.ico"));
                    break;
                default:
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/File Extentions/file-24.ico"));
                    break;

            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class UserToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                string usernickname = "";

                if (value is RestUser)
                    usernickname = ((RestUser)value).Username;
                else if (value is SocketSelfUser || value is SocketUser)
                    usernickname = ((SocketUser)value).Username;
                //else if (value is SocketUser)
                //    usernickname = ((SocketUser)value).Username;
                else
                {
                    usernickname = ((SocketGuildUser)value).Nickname;
                    if (usernickname == null)
                    {
                        usernickname = ((SocketGuildUser)value).Username;
                    }
                }
                if (usernickname.Length >= 18)
                {
                    usernickname = usernickname.Substring(0, 17) + "...";
                }

                return usernickname;
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class UserToUserColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Discord.Color userColor = new Discord.Color(255, 255, 255);

            if (value.GetType() == typeof(SocketGuildUser))
            {
                userColor = ((SocketGuildUser)value).Roles.OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? userColor;
            }
            else if (value.GetType() != typeof(RestUser) && value.GetType() != typeof(RestWebhookUser) && value.GetType() != typeof(SocketSelfUser))
            {
                userColor = ((IEnumerable<SocketRole>)value).OrderByDescending(x => x.Position).FirstOrDefault(x => x.Color.RawValue != Discord.Color.Default.RawValue)?.Color ?? userColor;
            }

            SolidColorBrush color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, userColor.R, userColor.G, userColor.B));

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class VoiceChannelToUsersCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string voicerUsersCount = "N/A";
            if (value != null)
            {
                voicerUsersCount = ((SocketVoiceChannel)value).Users.Count + " / ";
                if (((SocketVoiceChannel)value).UserLimit != null)
                    voicerUsersCount = voicerUsersCount + ((SocketVoiceChannel)value).UserLimit;
                else
                    voicerUsersCount = voicerUsersCount + "\u221E";
            }
            return voicerUsersCount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageToMessageInfoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IMessage previousMessage = null;

            if (ChatViewModel.MessageListCopy.Count > 1)
            {
                foreach (IMessage message in ChatViewModel.MessageListCopy)
                {
                    if (((IMessage)value).Id == message.Id)
                    {
                        if (previousMessage != null)
                        {
                            double timeDif = message.Timestamp.Subtract(previousMessage.Timestamp).TotalMinutes;
                            if ((previousMessage.Author == message.Author) && timeDif < 20)
                                return Visibility.Collapsed;
                            break;
                        }
                        else
                            return Visibility.Visible;
                    }
                    previousMessage = message;
                }
            }
            else
                return Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }

    public class VoiceChannelToVoiceChannelUsers : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((SocketVoiceChannel)value).Users;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NewGuildVoiceRegionToFlag : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                switch (((RestVoiceRegion)value).Name)
                {
                    case "Brazil":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Brazil.png"));
                    case "Central Europe":
                    case "Western Europe":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Europe.png"));
                    case "Hong Kong":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Hong Kong.png"));
                    case "Russia":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Russia.png"));
                    case "Singapore":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Singapore.png"));
                    case "US Central":
                    case "US West":
                    case "US South":
                    case "US East":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/USA.png"));
                    case "Syndey":
                        return new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/Flags/Australia.png"));
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
