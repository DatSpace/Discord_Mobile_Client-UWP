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

namespace Discord_Mobile.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        private int NumOfMessages = 30;
        public event PropertyChangedEventHandler PropertyChanged;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private SocketGuild Guild;
        private SocketTextChannel TextChannel;
        private SocketVoiceChannel VoiceChannel;
        private Collection<UsersTyping> UsersTyping = new Collection<UsersTyping>();
        
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void UpdateUserTypingStatus()
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
                foreach (var user in UsersTyping)
                {
                    if (Users_Typing == "")
                    {
                        Users_Typing += user.Username;
                    }
                    else
                        Users_Typing += ", " + user.Username;
                }

                if (Users_Typing == "")
                    Users_Typing_Visibility = false;
                else
                    Users_Typing_Visibility = true;
            });
        }

        public void Initialization()
        {
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
            {
                UpdateUserTypingStatus();
            }, TimeSpan.FromSeconds(2));

            SetUser();

            if (localSettings.Values["Enable_Sounds"] == null)
                localSettings.Values["Enable_Sounds"] = true;

            GuildsList = LoginService.client.Guilds;

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

            ScreenHorizontalCenter = ((int)ApplicationView.GetForCurrentView().VisibleBounds.Width / 10);
            ScreenVerticalCenter = ((int)ApplicationView.GetForCurrentView().VisibleBounds.Height / 4);

        }

        private async Task User_Typing(SocketUser arg1, ISocketMessageChannel arg2)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (TextChannel != null)
                    if (TextChannel == arg2)
                    {
                        if (!Users_Typing.Contains(arg1.Username))
                        {
                            UsersTyping.Add(new UsersTyping { Username = arg1.Username, TimeChecked = DateTime.Now });
                        }
                        else
                        {
                            foreach (var user in UsersTyping)
                            {
                                if (user.Username == arg1.Username)
                                {
                                    user.TimeChecked = DateTime.Now;
                                    break;
                                }
                            }
                        }
                        UpdateUserTypingStatus();
                    }
            });
        }

        private async Task User_Joined(SocketGuildUser arg)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Guild != null)
                {
                    if (Guild.Users.Count < 500)
                    {
                        GuildUserList.Clear();
                        foreach (SocketGuildUser user in Guild.Users)
                        {
                            if (user.Status != UserStatus.Offline)
                            {
                                GuildUserList.Add(user);
                            }
                        }
                    }
                }
            });
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
                UserAvatar = LoginService.client.CurrentUser.GetAvatarUrl();
                UserName = LoginService.client.CurrentUser.Username;
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
                     messageList.Add(arg);
                     if ((bool)localSettings.Values["Enable_Sounds"])
                     {
                         SoundPath = null;
                         SoundPath = new Uri("ms-appx://Discord_Mobile/Assets/new_message.wav");
                     }
                     if (messageList.Count > NumOfMessages)
                         messageList.Remove(messageList[0]);
                 });
            }
        }

        public void SelectGuild(object sender, ItemClickEventArgs e)
        {
            Guild = (SocketGuild)e.ClickedItem;
            NoGuildVisibility = Visibility.Collapsed;
            GuildChannelsList = Guild.Channels;
            SetChannels();
            GuildUserList.Clear();
            foreach (SocketGuildUser user in Guild.Users)
            {
                if (user.Status != UserStatus.Offline)
                {
                    GuildUserList.Add(user);
                }
            }
        }

        private void SetChannels()
        {
            TextChannelsList.Clear();
            VoiceChannelsList.Clear();
            foreach (SocketGuildChannel tempchannel in GuildChannelsList)
            {
                if (tempchannel is SocketTextChannel)
                {
                    TextChannelsList.Add(tempchannel);
                }
                else
                {
                    VoiceChannelsList.Add(tempchannel);
                }
            }

            TextChannelsList = OrderThoseGroups(TextChannelsList);
            VoiceChannelsList = OrderThoseGroups(VoiceChannelsList);
        }

        public static ObservableCollection<SocketGuildChannel> OrderThoseGroups(ObservableCollection<SocketGuildChannel> orderThoseGroups)
        {
            ObservableCollection<SocketGuildChannel> temp;
            temp = new ObservableCollection<SocketGuildChannel>(orderThoseGroups.OrderBy(p => p.Position));
            orderThoseGroups.Clear();
            foreach (SocketGuildChannel j in temp)
                orderThoseGroups.Add(j);
            return orderThoseGroups;
        }

        public async Task SelectTextChannel(object sender, ItemClickEventArgs e)
        {
            TextChannel = (SocketTextChannel)e.ClickedItem;
            NoChannelVisibility = Visibility.Collapsed;
            IEnumerable<IMessage> data = await TextChannel.GetMessagesAsync(NumOfMessages).Flatten();
            messageList.Clear();
            foreach (var item in data)
            {
                messageList.Add(item);
            }
            for (int i = 0; i < messageList.Count / 2; i++)
            {
                IMessage temp = messageList[i];
                messageList[i] = messageList[messageList.Count - i - 1];
                messageList[messageList.Count - i - 1] = temp;
            }
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            TopMessage = TextChannel.Name;
        }

        public async Task SelectVoiceChannel(object sender, ItemClickEventArgs e)
        {
            VoiceChannel = (SocketVoiceChannel)e.ClickedItem;
            var AudioClient = await VoiceChannel.ConnectAsync();


        }

        public void GoToSettings()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(SettingsView));
        }

        public async Task SendMessageToTextChannel()
        {
            await TextChannel?.SendMessageAsync(Message);
            Message = "";
        }

        public void ChannelsSplitViewPaneControl(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
        }

        public void UsersSplitViewPaneControl(object sender, RoutedEventArgs e)
        {
            UsersSplitViewPaneOpen = !UsersSplitViewPaneOpen;

        }

        public void AddGuildPopUp(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            AddGuildPopUpOpen = true;
        }

        public void JoinGuildPopUpOpen(object sender, RoutedEventArgs e)
        {

            AddGuildPopUpOpen = false;
            JoinGuildPopUpOpenProperty = true;
        }

        public void JoinGuildPopUpCancel(object sender, RoutedEventArgs e)
        {

            JoinGuildPopUpOpenProperty = false;
        }

        public void JoinGuildPopUpJoin(object sender, RoutedEventArgs e)
        {
            //Not Supported yet!
        }

        public void CreateGuildPopUpOpen(object sender, RoutedEventArgs e)
        {
            AddGuildPopUpOpen = false;
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
                Guild.CreateTextChannelAsync(NewChannelName);
                CreateChannelPopUpOpenProperty = false;
                NewChannelName = "";
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

        //######################################################################

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

        private string joinGuildName = "";

        public string JoinGuildName
        {
            get
            {
                return joinGuildName;
            }
            set
            {
                if (value != joinGuildName)
                {
                    joinGuildName = value;
                    NotifyPropertyChanged("JoinGuildName");
                }
            }
        }

        private int screenHorizontalCenter = 0;

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

        private int screenVerticalCenter = 0;

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

        private ObservableCollection<IMessage> messageList = new ObservableCollection<IMessage>();

        public ObservableCollection<IMessage> MessageList
        {
            get
            {
                return messageList;
            }
        }

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