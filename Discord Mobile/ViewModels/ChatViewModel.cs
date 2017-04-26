﻿using Discord_Mobile.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Discord;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Collections.ObjectModel;
using System;
using Windows.UI.Core;
using Discord_Mobile.Models;
using Windows.System.Threading;
using System.Linq;
using Windows.UI.ViewManagement;
using Discord.Audio;
using Windows.Storage.Streams;
using System.IO;

namespace Discord_Mobile.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {

        private int NumOfMessages = 30;
        public event PropertyChangedEventHandler PropertyChanged;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private SocketGuild Guild;
        private static SocketTextChannel TextChannel;
        public static SocketVoiceChannel VoiceChannel;
        private Collection<UsersTyping> UsersTyping = new Collection<UsersTyping>();
        private static SocketSelfUser User;
        private GuildPermissions GuildPermissions;
        private ChannelPermissions TextChannelPermissions;
        private ChannelPermissions VoiceChannelPermissions;


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void IsReady()
        {
            LoginService.client.Ready += Initialization;
        }

        private async Task Initialization()
        {
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
            {
                UpdateUserTypingStatus();
            }, TimeSpan.FromSeconds(2));

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                User = LoginService.client.CurrentUser;
                SetUser();
                GuildsList = LoginService.client.Guilds;
                LoadingPopUpIsOpen = false;
            });

            LoginService.client.MessageReceived += Message_Received;
            LoginService.client.MessageDeleted += Message_Deleted;
            LoginService.client.CurrentUserUpdated += User_Updated;
            LoginService.client.LeftGuild += Joined_Guild;
            LoginService.client.JoinedGuild += Joined_Guild;
            LoginService.client.ChannelCreated += Channel_Created;
            LoginService.client.ChannelDestroyed += Channel_Created;
            LoginService.client.UserJoined += User_Joined;
            LoginService.client.UserLeft += User_Joined;
            LoginService.client.UserIsTyping += User_Typing;
            //LoginService.client.Log += Client_Log;
        }

        //private Task Client_Log(LogMessage arg)
        //{
        //    if (VoiceChannel != null && arg.Message == "Disconnected")
        //        throw new Exception("Message: " + arg.Message + ", Severity: " + arg.Severity + ", Source: " + arg.Source);
        //    else
        //        return null;
        //}

        private async void UpdateUserTypingStatus()
        {
            if (TextChannel != null && UsersTyping != null)
            {
                for (int i = 0; i < UsersTyping.Count; i++)
                {
                    if (UsersTyping[i].ToBeDeleted())
                    {
                        UsersTyping.Remove(UsersTyping[i]);
                    }
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Users_Typing = "";

                    if (UsersTyping.Count >= 3)
                    {
                        Users_Typing_Visibility = true;
                        Users_Typing = "Several users are typing...";
                    }
                    else if (UsersTyping.Count == 2)
                    {
                        Users_Typing_Visibility = true;
                        Users_Typing = UsersTyping[0].Username + ", " + UsersTyping[1] + " are typing...";
                    }
                    else if (UsersTyping.Count == 1)
                    {
                        Users_Typing_Visibility = true;
                        Users_Typing = UsersTyping[0].Username + " is typing...";
                    }
                    else
                        Users_Typing_Visibility = false;

                    //foreach (var typinguser in UsersTyping)
                    //{
                    //    if (Users_Typing == "")
                    //    {
                    //        Users_Typing += typinguser.Username;
                    //    }
                    //    else
                    //        Users_Typing += ", " + typinguser.Username;
                    //}

                    //if (Users_Typing == "")
                    //    Users_Typing_Visibility = false;
                    //else
                    //    Users_Typing_Visibility = true;
                });
            }
        }

        private async Task User_Typing(SocketUser arg1, ISocketMessageChannel arg2)
        {
            if (TextChannel != null && TextChannel == arg2)
            {

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    if (!Users_Typing.Contains(arg1.Username))
                    {
                        UsersTyping.Add(new UsersTyping { Username = arg1.Username, TimeChecked = DateTime.Now });
                    }
                    else
                    {
                        foreach (var typinguser in UsersTyping)
                        {
                            if (typinguser.Username == arg1.Username)
                            {
                                typinguser.TimeChecked = DateTime.Now;
                                break;
                            }
                        }
                    }
                });
                UpdateUserTypingStatus();
            }
        }

        private async Task User_Joined(SocketGuildUser arg)
        {
            if (Guild != null && arg.Guild == Guild)
            {
                if (Guild.Users.Count < 500)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        GuildUserList.Clear();
                        foreach (SocketGuildUser guilduser in Guild.Users)
                        {
                            if (guilduser.Status != UserStatus.Offline)
                            {
                                GuildUserList.Add(guilduser);
                            }
                        }
                    });
                }
            }
        }


        private async Task Channel_Created(SocketChannel arg)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetChannels();
            });
        }

        private async Task Joined_Guild(SocketGuild arg)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GuildsList = LoginService.client.Guilds;
            });
        }

        private async Task Message_Deleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (arg2 == TextChannel)
            {
                //messageList.Remove(arg1);
            }
        }

        private async Task User_Updated(SocketSelfUser arg1, SocketSelfUser arg2)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetUser();
            });
        }

        private void SetUser()
        {
            try
            {
                UserAvatar = User.GetAvatarUrl();
                UserName = User.Username;
            }
            catch
            {
                MainChatImage = "/Assets/NoConnection.png";
                NoChannelMessage = "Connection failed\n Please restart me!";
            }
        }

        private async Task Message_Received(SocketMessage arg)
        {
            if (arg.Channel == TextChannel)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    for (int i = 0; i < UsersTyping.Count; i++)
                    {
                        if (UsersTyping[i].Username == arg.Author.Username)
                        {
                            UsersTyping.Remove(UsersTyping[i]);
                        }
                    }
                    MessageList.Add(arg);
                    if ((bool)localSettings.Values["Enable_Sounds"])
                    {
                        SoundPath = null;
                        SoundPath = new Uri("ms-appx://Discord_Mobile/Assets/new_message.wav");
                    }
                });
            }
        }

        public void SelectGuild(object sender, ItemClickEventArgs e)
        {
            LoadingPopUpIsOpen = true;
            Guild = (SocketGuild)e.ClickedItem;
            NoGuildVisibility = Visibility.Collapsed;
            GuildSelectedText = Guild.Name;
            GuildChannelsList = Guild.Channels;
            GuildPermissions = Guild.CurrentUser.GuildPermissions;
            SetChannels();
            HasModifyChannelPermission = Visibility.Collapsed;
            if (GuildPermissions.ManageChannels)
                HasModifyChannelPermission = Visibility.Visible;
            //GuildRoles = ;
            //Breaks Bind
            OrderRoles(Guild.Roles);
            LoadingPopUpIsOpen = false;
        }

        public void SetUsersList(object sender, TextChangedEventArgs e)
        {
            //if (Guild != null)
            //{
            //    GuildUserList.Clear();
            //    if (sender != null)
            //        SearchUsersText = ((TextBox)sender).Text;
            //    else
            //        SearchUsersText = "";
            //    foreach (SocketGuildUser guilduser in Guild.Users)
            //    {
            //        if (guilduser.Username != null && guilduser.Status != UserStatus.Offline && guilduser.Username.ToLower().StartsWith(SearchUsersText.ToLower()))
            //            GuildUserList.Add(guilduser);
            //    }
            //}
        }

        private void SetChannels()
        {
            TextChannelsList.Clear();
            VoiceChannelsList.Clear();
            foreach (SocketGuildChannel tempchannel in GuildChannelsList)
            {
                if (tempchannel is SocketTextChannel)
                {
                    if (Guild.CurrentUser.GetPermissions(tempchannel).ReadMessages)
                    {
                        TextChannelsList.Add(tempchannel);
                    }
                }
                else
                {
                    if (Guild.CurrentUser.GetPermissions(tempchannel).Connect)
                    {
                        VoiceChannelsList.Add(tempchannel);
                    }
                }
            }

            TextChannelsList = OrderChannels(TextChannelsList);
            VoiceChannelsList = OrderChannels(VoiceChannelsList);
        }

        private static ObservableCollection<SocketRole> OrderRoles(IReadOnlyCollection<SocketRole> roles)
        {
            ObservableCollection<SocketRole> temp;
            temp = new ObservableCollection<SocketRole>(roles.OrderBy(p => p.Position));
            return temp;
        }

        private static ObservableCollection<SocketGuildChannel> OrderChannels(ObservableCollection<SocketGuildChannel> channels)
        {
            ObservableCollection<SocketGuildChannel> temp;
            temp = new ObservableCollection<SocketGuildChannel>(channels.OrderBy(p => p.Position));
            channels.Clear();
            foreach (SocketGuildChannel j in temp)
                channels.Add(j);
            return channels;
        }

        public async Task SelectTextChannel(object sender, ItemClickEventArgs e)
        {
            LoadingPopUpIsOpen = true;
            TextChannel = (SocketTextChannel)e.ClickedItem;
            HasSendMessagePermission = false;
            TextChannelPermissions = Guild.CurrentUser.GetPermissions(TextChannel);
            NumOfMessages = 30;
            IEnumerable<IMessage> tempMessageList = await TextChannel.GetMessagesAsync(NumOfMessages).Flatten();
            MessageList.Clear();
            foreach (var item in tempMessageList)
            {
                MessageList.Insert(0, item);
            }
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            TopMessage = TextChannel.Name;
            if (TextChannelPermissions.SendMessages)
            {
                HasSendMessagePermission = true;
            }
            if (MessageList.Count > 0)
                NoChannelVisibility = Visibility.Collapsed;
            else
                NoChannelMessage = "No Messages Yet :/";
            LoadingPopUpIsOpen = false;
        }

        public async void LoadMoreMessages()
        {
            NumOfMessages += 30;
            IEnumerable<IMessage> tempMessageList = await TextChannel.GetMessagesAsync(NumOfMessages).Flatten();
            foreach (IMessage item in tempMessageList)
            {
                //Compare the unique message id's, otherwise it's not working!
                if (!MessageList.Any(x => x.Id == item.Id))
                {
                    MessageList.Insert(0, item);
                }
            }
        }

        public async Task SelectVoiceChannel(object sender, ItemClickEventArgs e)
        {
            VoiceChannel = (SocketVoiceChannel)e.ClickedItem;

            //await Task.Run(() => VoiceService.Initialize());

            //if (VoiceChannelPermissions.Speak)
            //{
            //}
        }

        public void SettingsPopUpOpen()
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            SettingsPopUpOpenProperty = true;
        }

        public async Task SendMessageToTextChannel()
        {
            LoadingPopUpIsOpen = true;
            await TextChannel?.SendMessageAsync(Message);
            Message = "";
            LoadingPopUpIsOpen = false;
        }

        public void ChannelsSplitViewPaneControl(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
        }

        public void UsersSplitViewPaneControl(object sender, RoutedEventArgs e)
        {
            UsersSplitViewPaneOpen = !UsersSplitViewPaneOpen;

        }

        public void CreateGuildPopUpOpen(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            CreateGuildPopUpOpenProperty = true;
        }

        public void CreateGuildPopUpCancel(object sender, RoutedEventArgs e)
        {
            CreateGuildPopUpOpenProperty = false;
        }

        public void CreateGuildPopUpCreate(object sender, RoutedEventArgs e)
        {
            if (NewGuildName != "" && NewGuildServer != "Select Server")
            {
                foreach (var region in LoginService.client.VoiceRegions)
                {
                    if (region.Name == NewGuildServer)
                    {
                        LoginService.client.CreateGuildAsync(NewGuildName, region);
                        CreateGuildPopUpOpenProperty = false;
                        NewGuildName = "";
                        NewGuildServer = "Select Server";
                    }
                }
            }
        }

        public void CreateChannelPopUpCreate(object sender, RoutedEventArgs e)
        {
            if (NewChannelName != "" && Guild != null)
            {
                LoadingPopUpIsOpen = true;
                Guild.CreateTextChannelAsync(NewChannelName);
                CreateChannelPopUpOpenProperty = false;
                NewChannelName = "";
                LoadingPopUpIsOpen = false;
            }
        }

        public void CreateChannelPopUpOpen(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            CreateChannelPopUpOpenProperty = true;
        }

        public void CreateChannelPopUpCancel(object sender, RoutedEventArgs e)
        {

            CreateChannelPopUpOpenProperty = false;
        }

        public void CreateGuildServerSelect(object sender, RoutedEventArgs e)
        {
            var selection = ((MenuFlyoutItem)sender).Text;
            NewGuildServer = selection;
        }

        public void SetPopUpCenter(object sender, object e)
        {
            ScreenHorizontalCenter = (int)((ApplicationView.GetForCurrentView().VisibleBounds.Width / 2) - (((Grid)sender).ActualWidth / 2));
            ScreenVerticalCenter = (int)((ApplicationView.GetForCurrentView().VisibleBounds.Height / 2) - (((Grid)sender).ActualHeight / 2));
        }

        //######################################################################

        public ObservableCollection<SocketRole> GuildRoles;

        //public IReadOnlyCollection<SocketRole> GuildRoles
        //{
        //    get
        //    {
        //        return guildRoles;
        //    }
        //    set
        //    {
        //        if (value != guildRoles)
        //        {
        //            guildRoles = value;
        //            NotifyPropertyChanged("GuildRoles");
        //        }
        //    }
        //}

        private string searchUserText = "";

        public string SearchUsersText
        {
            get
            {
                return searchUserText;
            }
            set
            {
                if (value != searchUserText)
                {
                    searchUserText = value;
                    NotifyPropertyChanged("SearchUserText");
                }
            }
        }

        private bool loadingPopUpIsOpen = true;

        public bool LoadingPopUpIsOpen
        {
            get
            {
                return loadingPopUpIsOpen;
            }
            set
            {
                if (value != loadingPopUpIsOpen)
                {
                    loadingPopUpIsOpen = value;
                    NotifyPropertyChanged("LoadingPopUpIsOpen");
                }
            }
        }

        private Visibility hasModifyChannelPermission = Visibility.Collapsed;

        public Visibility HasModifyChannelPermission
        {
            get
            {
                return hasModifyChannelPermission;
            }
            set
            {
                if (value != hasModifyChannelPermission)
                {
                    hasModifyChannelPermission = value;
                    NotifyPropertyChanged("HasModifyChannelPermission");
                }
            }
        }

        private bool hasSendMessagePermission = false;

        public bool HasSendMessagePermission
        {
            get
            {
                return hasSendMessagePermission;
            }
            set
            {
                if (value != hasSendMessagePermission)
                {
                    hasSendMessagePermission = value;
                    NotifyPropertyChanged("HasSendMessagePermission");
                }
            }
        }

        private bool settingsPopUpOpenProperty = false;

        public bool SettingsPopUpOpenProperty
        {
            get
            {
                return settingsPopUpOpenProperty;
            }
            set
            {
                if (value != settingsPopUpOpenProperty)
                {
                    settingsPopUpOpenProperty = value;
                    NotifyPropertyChanged("SettingsPopUpOpenProperty");
                }
            }
        }

        private string guildSelectedText = "Select Guild";

        public string GuildSelectedText
        {
            get
            {
                return guildSelectedText;
            }
            set
            {
                if (value != guildSelectedText)
                {
                    guildSelectedText = value;
                    NotifyPropertyChanged("GuildSelectedText");
                }
            }
        }

        private string newGuildServer = "Select Server";

        public string NewGuildServer
        {
            get
            {
                return newGuildServer;
            }
            set
            {
                if (value != newGuildServer)
                {
                    newGuildServer = value;
                    NotifyPropertyChanged("NewGuildServer");
                }
            }
        }

        private string newGuildName = "";

        public string NewGuildName
        {
            get
            {
                return newGuildName;
            }
            set
            {
                if (value != newGuildName)
                {
                    newGuildName = value;
                    NotifyPropertyChanged("NewGuildName");
                }
            }
        }

        private string newChannelName = "";

        public string NewChannelName
        {
            get
            {
                return newChannelName;
            }
            set
            {
                if (value != newChannelName)
                {
                    newChannelName = value;
                    NotifyPropertyChanged("NewChannelName");
                }
            }
        }

        private int screenHorizontalCenter = 1;

        public int ScreenHorizontalCenter
        {
            get
            {
                return screenHorizontalCenter;
            }
            set
            {
                if (value != screenHorizontalCenter)
                {
                    screenHorizontalCenter = value;
                    NotifyPropertyChanged("ScreenHorizontalCenter");
                }
            }
        }

        private int screenVerticalCenter = 1;

        public int ScreenVerticalCenter
        {
            get
            {
                return screenVerticalCenter;
            }
            set
            {
                if (value != screenVerticalCenter)
                {
                    screenVerticalCenter = value;
                    NotifyPropertyChanged("ScreenVerticalCenter");
                }
            }
        }

        private bool createChannelPopUpOpenProperty = false;

        public bool CreateChannelPopUpOpenProperty
        {
            get
            {
                return createChannelPopUpOpenProperty;
            }
            set
            {
                if (value != createChannelPopUpOpenProperty)
                {
                    createChannelPopUpOpenProperty = value;
                    NotifyPropertyChanged("CreateChannelPopUpOpenProperty");
                }
            }
        }

        private bool createGuildPopUpOpenProperty = false;

        public bool CreateGuildPopUpOpenProperty
        {
            get
            {
                return createGuildPopUpOpenProperty;
            }
            set
            {
                if (value != createGuildPopUpOpenProperty)
                {
                    createGuildPopUpOpenProperty = value;
                    NotifyPropertyChanged("CreateGuildPopUpOpenProperty");
                }
            }
        }

        private bool joinGuildPopUpOpenProperty = false;

        public bool JoinGuildPopUpOpenProperty
        {
            get
            {
                return joinGuildPopUpOpenProperty;
            }
            set
            {
                if (value != joinGuildPopUpOpenProperty)
                {
                    joinGuildPopUpOpenProperty = value;
                    NotifyPropertyChanged("JoinGuildPopUpOpenProperty");
                }
            }
        }

        private bool addGuildPopUpOpen = false;

        public bool AddGuildPopUpOpen
        {
            get
            {
                return addGuildPopUpOpen;
            }
            set
            {
                if (value != addGuildPopUpOpen)
                {
                    addGuildPopUpOpen = value;
                    NotifyPropertyChanged("AddGuildPopUpOpen");
                }
            }
        }

        private Uri soundPath = null;

        public Uri SoundPath
        {
            get
            {
                return soundPath;
            }
            set
            {
                if (value != soundPath)
                {
                    soundPath = value;
                    NotifyPropertyChanged("SoundPath");
                }
            }
        }
        private string message = "";

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (value != message)
                {
                    message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

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
                    if (value == null)
                        useravatar = "/Assets/NoAvatarIcon.png";
                    else
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

        private IReadOnlyCollection<SocketGuildChannel> guildChannelsList = null;

        public IReadOnlyCollection<SocketGuildChannel> GuildChannelsList
        {
            get
            {
                return guildChannelsList;
            }
            set
            {
                if (value != guildChannelsList)
                {
                    guildChannelsList = value;
                    NotifyPropertyChanged("GuildChannelsList");
                }
            }
        }

        public ObservableCollection<SocketGuildUser> GuildUserList = new ObservableCollection<SocketGuildUser>();

        public ObservableCollection<SocketGuildChannel> TextChannelsList = new ObservableCollection<SocketGuildChannel>();
        public ObservableCollection<SocketGuildChannel> VoiceChannelsList = new ObservableCollection<SocketGuildChannel>();

        private string users_Typing = "";

        public string Users_Typing
        {
            get
            {
                return users_Typing;
            }
            set
            {
                if (value != users_Typing)
                {
                    users_Typing = value;
                    NotifyPropertyChanged("Users_Typing");
                }
            }
        }

        private bool users_Typing_Visibility = false;

        public bool Users_Typing_Visibility
        {
            get
            {
                return users_Typing_Visibility;
            }
            set
            {
                if (value != users_Typing_Visibility)
                {
                    users_Typing_Visibility = value;
                    NotifyPropertyChanged("Users_Typing_Visibility");
                }
            }
        }

        private Visibility noChannelVisibility = Visibility.Visible;

        public Visibility NoChannelVisibility
        {
            get
            {
                return noChannelVisibility;
            }
            set
            {
                if (value != noChannelVisibility)
                {
                    noChannelVisibility = value;
                    NotifyPropertyChanged("NoChannelVisibility");
                }
            }
        }

        private string noChannelMessage = "No channel selected!";
        public string MainChatImage = "/Assets/Square150x150Logo.png";

        public string NoChannelMessage
        {
            get
            {
                return noChannelMessage;
            }
            set
            {
                if (value != noChannelMessage)
                {
                    noChannelMessage = value;
                    NotifyPropertyChanged("NoChannelMessage");
                    NotifyPropertyChanged("MainChatImage");
                }
            }
        }

        private Visibility noGuildVisibility = Visibility.Visible;

        public Visibility NoGuildVisibility
        {
            get
            {
                return noGuildVisibility;
            }
            set
            {
                if (value != noGuildVisibility)
                {
                    noGuildVisibility = value;
                    NotifyPropertyChanged("NoGuildVisibility");

                }
            }
        }

        public ObservableCollection<IMessage> MessageList = new ObservableCollection<IMessage>();

        private bool channelsSplitViewPaneOpen = false;

        public bool ChannelsSplitViewPaneOpen
        {
            get
            {
                return channelsSplitViewPaneOpen;
            }
            set
            {
                if (value != channelsSplitViewPaneOpen)
                {
                    channelsSplitViewPaneOpen = value;
                    NotifyPropertyChanged("ChannelsSplitViewPaneOpen");

                }
            }
        }

        private bool usersSplitViewPaneOpen = false;

        public bool UsersSplitViewPaneOpen
        {
            get
            {
                return usersSplitViewPaneOpen;
            }
            set
            {
                if (value != usersSplitViewPaneOpen)
                {
                    usersSplitViewPaneOpen = value;
                    NotifyPropertyChanged("UsersSplitViewPaneOpen");

                }
            }
        }

        private string topMessage = "Discord Mobile Client";

        public string TopMessage
        {
            get
            {
                return topMessage;
            }
            set
            {
                if (value != topMessage)
                {
                    topMessage = value;
                    NotifyPropertyChanged("TopMessage");

                }
            }
        }
    }
}