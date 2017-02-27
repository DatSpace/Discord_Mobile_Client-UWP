using Discord_Mobile.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Discord_Mobile.Models;
using Discord;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Collections.Generic;
using Discord.WebSocket;

namespace Discord_Mobile.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        UserModel user = new UserModel();
        GuildModel guilds = new GuildModel();

        public void SetUser()
        {
            UserAvatar = user.GetAvatar();
            UserName = user.GetName();
            UserStatus = user.GetStatus();
        }
        public void SetGuilds()
        {
            GuildsList = guilds.GetGuilds();
        }

        public async Task Disconnect()
        {
            //LoginService loginService = new LoginService();
            LoginService.DeleteUser();

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(LoginPage));
        }

        //######################################################################

        private string useravatar = "/Assets/NoAvatarIcon.png";

        public string UserAvatar
        {
            get
            {
                return useravatar;
            }
            set
            {
                if (value != useravatar)
                {
                    useravatar = value;
                    NotifyPropertyChanged("UserAvatar");
                }
            }
        }

        private string userName = "Uknown";

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                if (value != userName)
                {
                    userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        private UserStatus userStatus = UserStatus.Unknown;
        public SolidColorBrush userStatusColor = new SolidColorBrush(Colors.White);

        public UserStatus UserStatus
        {
            get
            {
                return userStatus;
            }
            set
            {
                if (value != userStatus)
                {
                    userStatus = value;
                    if (userStatus == UserStatus.Online)
                        userStatusColor = new SolidColorBrush(Colors.Green);
                    else if (userStatus == UserStatus.Idle)
                        userStatusColor = new SolidColorBrush(Colors.Orange);
                    else if (userStatus == UserStatus.DoNotDisturb)
                        userStatusColor = new SolidColorBrush(Colors.Red);
                    else if (userStatus == UserStatus.Invisible)
                        userStatusColor = new SolidColorBrush(Colors.DarkGray);
                    NotifyPropertyChanged("UserStatus");
                    NotifyPropertyChanged("userStatusColor");
                }
            }
        }

        private IReadOnlyCollection<SocketGuild> guildsList = null;

        public IReadOnlyCollection<SocketGuild> GuildsList
        {
            get
            {
                return guildsList;
            }
            set
            {
                if (value != guildsList)
                {
                    guildsList = value;
                    NotifyPropertyChanged("GuildsList");
                }
            }
        }
    }
}
