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

namespace Discord_Mobile.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush color;
            switch (value)
            {
                case UserStatus.Offline:
                    color = new SolidColorBrush(Colors.LightGray);
                    break;
                case UserStatus.Online:
                    color = new SolidColorBrush(Colors.SpringGreen);
                    break;
                case UserStatus.Idle:
                case UserStatus.AFK:
                    color = new SolidColorBrush(Colors.Orange);
                    break;
                case UserStatus.DoNotDisturb:
                    color = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    color = new SolidColorBrush(Colors.LightGray);
                    break;
            }
            return color;
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
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/pdf-24.ico"));
                    break;
                case ".csv":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/csv-24.ico"));
                    break;
                case ".txt":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/document-24.ico"));
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
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/exel-24.ico"));
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
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/powerpoint-3-24.ico"));
                    break;
                case ".doc":
                case ".dot":
                case ".wbk":
                case ".docx":
                case ".docm":
                case ".dotx":
                case ".dotm":
                case ".docb":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/word-3-24.ico"));
                    break;
                case ".avi":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/avi-24.ico"));
                    break;
                case ".flv":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/flv-24.ico"));
                    break;
                case ".mov":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/mov-24.ico"));
                    break;
                case ".mpg":
                case ".mpeg":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/mpg-24.ico"));
                    break;
                case ".dll":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/dll-24.ico"));
                    break;
                case ".exe":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/exe-24.ico"));
                    break;
                case ".psd":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/psd-24.ico"));
                    break;
                case ".gif":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/gif-24.ico"));
                    break;
                case ".jpg":
                case ".jpeg":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/jpg-24.ico"));
                    break;
                case ".png":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/png-24.ico"));
                    break;
                case ".rar":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/rar-24.ico"));
                    break;
                case ".zip":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/zip-24.ico"));
                    break;
                case ".mp3":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/mp3-24.ico"));
                    break;
                case ".wma":
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/wma-24.ico"));
                    break;
                default:
                    image = new BitmapImage(new Uri("ms-appx://Discord_Mobile/Assets/file-24.ico"));
                    break;

            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class AuthorToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                string usernickname = "";

                if (value is RestUser)
                    usernickname = ((RestUser)value).Username;
                else if (value is SocketSelfUser)
                {
                    usernickname = ((SocketSelfUser)value).Username;
                }
                else
                {
                    usernickname = ((SocketGuildUser)value).Nickname;
                    if (usernickname == null)
                    {
                        usernickname = ((SocketGuildUser)value).Username;
                    }
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

    public class RecipientToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush color;
            switch (((IUser)value).Status)
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

    public class UserToNicknameOrUsernameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = "";

            if (((SocketGuildUser)value).Nickname != null)
                name = ((SocketGuildUser)value).Nickname;
            else
                name = ((SocketGuildUser)value).Username;


            if (name.Length >= 18)
            {
                name = name.Substring(0, 17) + "...";
            }

            return name;

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
                else if (value.GetType() == typeof(RestUser))
                    url = ((RestUser)value).GetAvatarUrl();
                else if (value.GetType() == typeof(RestWebhookUser))
                    url = ((RestWebhookUser)value).GetAvatarUrl();
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

    public class RecipientToUsernameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((IUser)value).Username;
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
}
