using Discord_Mobile.Services;
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
using System.IO;
using Windows.UI.Xaml.Input;
using Windows.Networking.Connectivity;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media;

namespace Discord_Mobile.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        private int GuildsLoadedNumber = 0;
        private int NumOfMessages = 30;
        public event PropertyChangedEventHandler PropertyChanged;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public static SocketGuild Guild;
        private SocketTextChannel TextChannel;
        private static SocketVoiceChannel VoiceChannel;
        private Collection<UsersTyping> UsersTypingCollection = new Collection<UsersTyping>();
        private SocketSelfUser User;
        private GuildPermissions GuildPermissions;
        private ChannelPermissions TextChannelPermissions;
        private IDMChannel DMChannel;
        private Windows.Storage.Pickers.FileOpenPicker FilePicker = new Windows.Storage.Pickers.FileOpenPicker();
        private Stream PickedFileStream;
        public static Windows.Storage.StorageFile PickedFile;
        //private ChannelPermissions VoiceChannelPermissions;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void IsReady()
        {
            LoginService.client.Ready += InitializationAsync;
            LoginService.client.GuildAvailable += GuildLoadedAsync;
            NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;
        }

        private void NetworkStatusChanged(object sender)
        {
            var connectionStatus = NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel();
            if (connectionStatus != NetworkConnectivityLevel.InternetAccess)
                throw new NotImplementedException(connectionStatus.ToString());
        }

        private Task GuildLoadedAsync(SocketGuild arg)
        {
            GuildsLoadedNumber++;
            return null;
        }

        private async Task InitializationAsync()
        {
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
            {
                UpdateUserTypingStatus();
            }, TimeSpan.FromSeconds(3));

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                User = LoginService.client.CurrentUser;
                SetUser();
                foreach (SocketGuild guild in LoginService.client.Guilds)
                    GuildsList.Add(guild);
                while (GuildsLoadedNumber < GuildsList.Count)
                {
                }
                LoadingPopUpIsOpen = false;
            });

            LoginService.client.MessageReceived += MessageReceived;
            LoginService.client.MessageDeleted += MessageDeleted;
            LoginService.client.MessageUpdated += MessageEdited;
            LoginService.client.CurrentUserUpdated += UserUpdated;
            LoginService.client.LeftGuild += JoinedGuild;
            LoginService.client.JoinedGuild += JoinedGuild;
            LoginService.client.ChannelCreated += ChannelCreated;
            LoginService.client.ChannelDestroyed += ChannelDestroyed;
            LoginService.client.UserJoined += UserJoined;
            LoginService.client.UserLeft += UserJoined;
            LoginService.client.UserIsTyping += UserTypingEvent;
            LoginService.client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            //LoginService.client.Log += ClientLog;

            FilePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            FilePicker.FileTypeFilter.Add("*");
            FilePicker.CommitButtonText = "Send File";
        }
        
        private Task ClientLogAsync(LogMessage arg)
        {
            throw new Exception("Message: " + arg.Message + ", Severity: " + arg.Severity + ", Source: " + arg.Source);
        }

        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg2.VoiceChannel != null)
            {
                if (arg2.VoiceChannel.Guild == Guild)
                {
                    if (arg2.VoiceChannel.Users != arg2.VoiceChannel.Users)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            VoiceChannelsList.Clear();
                            foreach (SocketGuildChannel tempchannel in Guild.Channels.OrderBy(channel => channel.Position))
                            {
                                if (tempchannel is SocketVoiceChannel)
                                {
                                    if (Guild.CurrentUser.GetPermissions(tempchannel).Connect)
                                    {
                                        VoiceChannelsList.Add(tempchannel);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                if (arg3.VoiceChannel.Guild == Guild)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VoiceChannelsList.Clear();
                        foreach (SocketGuildChannel tempchannel in Guild.Channels.OrderBy(channel => channel.Position))
                        {
                            if (tempchannel is SocketVoiceChannel)
                            {
                                if (Guild.CurrentUser.GetPermissions(tempchannel).Connect)
                                {
                                    VoiceChannelsList.Add(tempchannel);
                                }
                            }
                        }
                    });
                }
            }
        }

        private async Task UpdateUserTypingStatus()
        {
            if ((TextChannel != null || DMChannel != null) && PickedFile == null)
            {
                for (int i = 0; i < UsersTypingCollection.Count; i++)
                {
                    if (UsersTypingCollection[i].ToBeDeleted())
                    {
                        UsersTypingCollection.Remove(UsersTypingCollection[i]);
                    }
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    UsersTypingString = "";

                    if (UsersTypingCollection.Count >= 3)
                    {
                        UsersTypingString = "Several users are typing...";
                        UsersTypingVisibility = true;
                    }
                    else if (UsersTypingCollection.Count == 2)
                    {
                        UsersTypingString = UsersTypingCollection[0].Username + ", " + UsersTypingCollection[1].Username + " are typing...";
                        UsersTypingVisibility = true;
                    }
                    else if (UsersTypingCollection.Count == 1)
                    {
                        UsersTypingString = UsersTypingCollection[0].Username + " is typing...";
                        UsersTypingVisibility = true;
                    }
                    else
                        UsersTypingVisibility = false;
                });
            }
        }

        private async Task UserTypingEvent(SocketUser arg1, ISocketMessageChannel arg2)
        {
            if (DMChannel == arg2 || TextChannel == arg2)
            {
                if (!UsersTypingString.Contains(arg1.Username))
                {
                    UsersTypingCollection.Add(new UsersTyping { Username = arg1.Username, TimeChecked = DateTime.Now });
                }
                else
                {
                    foreach (var typinguser in UsersTypingCollection)
                    {
                        if (typinguser.Username == arg1.Username)
                        {
                            typinguser.TimeChecked = DateTime.Now;
                            break;
                        }
                    }
                }
                await UpdateUserTypingStatus();
            }
        }

        private async Task UserJoined(SocketGuildUser arg)
        {
            if (Guild != null && arg.Guild == Guild)
            {
                if (Guild.Users.Count < 200)
                {
                    FilterUsersList(null, null);
                }
            }
        }


        private async Task ChannelCreated(SocketChannel arg)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (arg.GetType() == typeof(SocketDMChannel))
                    DMChannelsList.Add((IDMChannel)arg);
                else
                    SetChannels();
            });
        }
        private async Task ChannelDestroyed(SocketChannel arg)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (arg.GetType() == typeof(SocketDMChannel))
                    DMChannelsList.Remove((IDMChannel)arg);
                else
                    SetChannels();
            });
        }

        private async Task JoinedGuild(SocketGuild arg)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (SocketGuild guild in LoginService.client.Guilds)
                    GuildsList.Add(guild);
            });
        }

        private async Task MessageEdited(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (arg3 == TextChannel || arg3 == DMChannel)
            {
                if (arg1.HasValue)
                {
                    int index = MessageList.IndexOf(arg2);
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageList[index] = arg2;
                        MessageListCopy[index] = arg2;
                    });
                }
                else
                {
                    ulong messageId = arg1.Id;
                    foreach (IMessage message in MessageList)
                    {
                        if (message.Id == messageId)
                        {
                            int index = MessageList.IndexOf(message);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                MessageList[index] = arg2;
                                MessageListCopy[index] = arg2;
                            });
                            break;
                        }
                    }
                }
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (arg2 == TextChannel || arg2 == DMChannel)
            {
                if (arg1.HasValue)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageList.Remove(arg1.Value);
                        MessageListCopy.Remove(arg1.Value);
                        if (!MessageList.Any())
                        {
                            NoChannelVisibility = Visibility.Visible;
                            NoChannelMessage = "No Messages...";
                        }
                    });
                }
                else
                {
                    ulong messageId = arg1.Id;
                    foreach (IMessage message in MessageList)
                    {
                        if (message.Id == messageId)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                MessageList.Remove(message);
                                MessageListCopy.Remove(message);
                                if (!MessageList.Any())
                                {
                                    NoChannelVisibility = Visibility.Visible;
                                    NoChannelMessage = "No Messages...";
                                }
                            });
                            break;
                        }
                    }
                }
            }
        }

        private async Task UserUpdated(SocketSelfUser arg1, SocketSelfUser arg2)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Channel == DMChannel || arg.Channel == TextChannel)
            {
                if (((IMessage)arg).Type == MessageType.Default)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        for (int i = 0; i < UsersTypingCollection.Count; i++)
                        {
                            if (UsersTypingCollection[i].Username == arg.Author.Username)
                            {
                                UsersTypingCollection.Remove(UsersTypingCollection[i]);
                            }
                        }
                        MessageList.Add(arg);
                        MessageListCopy.Add(arg);
                        if ((bool)localSettings.Values["EnableSounds"])
                        {
                            SoundPath = null;
                            SoundPath = new Uri("ms-appx://Discord_Mobile/Assets/new_message.wav");
                        }

                        NoChannelVisibility = Visibility.Collapsed;

                    });
                }
            }
        }

        private void ClearGuild()
        {
            if (Guild != null)
            {
                GroupedGuildUsers = null;
                NotifyPropertyChanged("GroupedGuildUsers");

                TextChannelsList.Clear();
                VoiceChannelsList.Clear();
                MessageList.Clear();
                MessageListCopy.Clear();
                PinnedMessageList.Clear();

                Guild = null;
                TextChannel = null;
                VoiceChannel = null;
                GuildSettingsButtonVisibility = Visibility.Collapsed;
                HasSendMessagePermission = false;
                TopMessage = "Discord Mobile Client";
            }
        }

        public async void DisplayLeaveGuildDialog(object sender, RoutedEventArgs e)
        {
            ContentDialog leaveGuildDialog = new ContentDialog
            {
                Title = "Are you sure ?",
                PrimaryButtonText = "I am sure!",
                CloseButtonText = "It was a mistake",
                DefaultButton = ContentDialogButton.Close
            };
            ContentDialogResult result = await leaveGuildDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LeaveGuild();
                });
            }

        }

        private void LeaveGuild()
        {
            LoadingPopUpIsOpen = true;

            if (GuildPermissions.Administrator)
                Guild.DeleteAsync();
            else
                Guild.LeaveAsync();
            GuildSettingsPopUpOpenProperty = false;

            ClearGuild();

            NoChannelMessage = "No channel selected!";
            NoChannelVisibility = Visibility.Visible;
            NoGuildVisibility = Visibility.Visible;
            GuildSelectedText = "Select Guild";

            GuildSettingsButtonVisibility = Visibility.Collapsed;
            HasModifyChannelPermission = Visibility.Collapsed;
            HasSendMessagePermission = false;

            LoadingPopUpIsOpen = false;
        }

        public void SelectGuild(object sender, ItemClickEventArgs e)
        {
            LoadingPopUpIsOpen = true;
            Guild = (SocketGuild)e.ClickedItem;
            NoGuildVisibility = Visibility.Collapsed;
            GuildSelectedText = Guild.Name;
            GuildPermissions = Guild.CurrentUser.GuildPermissions;
            SetChannels();
            GuildSettingsButtonVisibility = Visibility.Visible;
            if (GuildPermissions.Administrator)
                GuildSettingsLeaveDeleteText = "Delete \"" + Guild.Name + "\"";
            else
                GuildSettingsLeaveDeleteText = "Leave \"" + Guild.Name + "\"";
            HasModifyChannelPermission = Visibility.Collapsed;
            ChannelsVisibility = Visibility.Visible;
            PrivateMessagesVisibility = Visibility.Collapsed;
            if (GuildPermissions.ManageChannels)
                HasModifyChannelPermission = Visibility.Visible;
            FilterUsersList(null, null);
            LoadingPopUpIsOpen = false;
        }

        public void FilterUsersList(object sender, TextChangedEventArgs e)
        {
            if (Guild != null)
            {
                GuildUserList.Clear();
                if (sender != null)
                    SearchUsersText = ((TextBox)sender).Text;
                else
                    SearchUsersText = "";
                foreach (SocketGuildUser guilduser in Guild.Users)
                {
                    if (guilduser.Username != null && guilduser.Username.ToLower().StartsWith(SearchUsersText.ToLower()))
                        GuildUserList.Add(guilduser);
                }
                GroupedGuildUsers = from user in GuildUserList.Where(user => user.Status != UserStatus.Offline) group user by user.Roles.Where(role => role.IsHoisted || role.IsEveryone).OrderByDescending(x => x.Position).First() into grp orderby grp.Key.Position descending select grp;
                NotifyPropertyChanged("GroupedGuildUsers");
            }
        }

        public async void UserTypingEventTrigger(object sender, TextChangedEventArgs e)
        {
            await TextChannel.TriggerTypingAsync();
        }

        private void SetChannels()
        {
            TextChannelsList.Clear();
            VoiceChannelsList.Clear();
            foreach (SocketGuildChannel tempchannel in Guild.Channels.OrderBy(channel => channel.Position))
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
        }

        public async Task SelectTextChannel(object sender, ItemClickEventArgs e)
        {
            LoadingPopUpIsOpen = true;
            TextChannel = (SocketTextChannel)e.ClickedItem;
            HasSendMessagePermission = false;
            HasSendFilePermission = false;
            TextChannelPermissions = Guild.CurrentUser.GetPermissions(TextChannel);
            NumOfMessages = 30;
            IEnumerable<IMessage> tempMessageList = await TextChannel.GetMessagesAsync(NumOfMessages).Flatten();
            MessageList.Clear();
            MessageListCopy.Clear();
            foreach (var item in tempMessageList)
            {
                MessageList.Insert(0, item);
                MessageListCopy.Insert(0, item);
            }
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            TopMessage = TextChannel.Name;
            if (TextChannelPermissions.SendMessages)
                HasSendMessagePermission = true;
            if (TextChannelPermissions.AttachFiles)
                HasSendFilePermission = true;
            NoChannelVisibility = Visibility.Visible;
            if (MessageList.Any())
                NoChannelVisibility = Visibility.Collapsed;
            else
                NoChannelMessage = "No Messages...";
            IEnumerable<IMessage> tempPinnedMessageList = await TextChannel.GetPinnedMessagesAsync();
            PinnedMessageList.Clear();
            foreach (var pinnedMessage in tempPinnedMessageList)
                PinnedMessageList.Add(pinnedMessage);
            LoadingPopUpIsOpen = false;
        }

        public async void LoadMoreMessages()
        {
            NumOfMessages += 30;
            IEnumerable<IMessage> tempMessageList = (IEnumerable<IMessage>)await TextChannel.GetMessagesAsync(NumOfMessages).Flatten();
            foreach (IMessage message in tempMessageList)
            {
                //Compare the unique message id's, otherwise it's not working!
                if (!MessageList.Any(x => x.Id == message.Id))
                {
                    MessageList.Insert(0, message);
                    MessageListCopy.Insert(0, message);
                }
            }
        }

        public async Task SelectVoiceChannel(object sender, ItemClickEventArgs e)
        {
            VoiceChannel = (SocketVoiceChannel)e.ClickedItem;

            //await Task.Run(() => VoiceService.Initialize(VoiceChannel));

            //if (VoiceChannelPermissions.Speak)
            //{
            //}
        }

        public void SettingsPopUpOpen()
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            SettingsPopUpOpenProperty = true;
        }

        public async Task SendMessage()
        {
            LoadingPopUpIsOpen = true;

            if (PickedFile != null)
            {
                if (TextChannel != null)
                    await TextChannel.SendFileAsync(PickedFileStream, PickedFile.Name, TextBoxMessage);
                else
                    await DMChannel.SendFileAsync(PickedFileStream, PickedFile.Name, TextBoxMessage);
                TextBoxMessage = "";
                PickedFile = null;
                PickedFileStream = null;
                AttachmentColorString = "White";
                AttachmentIconString = "\uE898";
                AttachmentPathString = "";
                NotifyPropertyChanged("TopMessage");
            }
            else if (TextBoxMessage.Trim() != "")
            {
                if (TextChannel != null)
                    await TextChannel.SendMessageAsync(TextBoxMessage);
                else
                    await DMChannel.SendMessageAsync(TextBoxMessage);
                TextBoxMessage = "";
            }

            LoadingPopUpIsOpen = false;
        }

        public async Task SelectFile()
        {
            LoadingPopUpIsOpen = true;

            if (PickedFile == null)
            {
                PickedFile = await FilePicker.PickSingleFileAsync();
                if (PickedFile != null)
                {
                    PickedFileStream = await PickedFile.OpenStreamForReadAsync();
                    AttachmentColorString = "DarkRed";
                    AttachmentIconString = "\uE74D";
                    AttachmentPathString = string.Format("File Attached: " + PickedFile.Path);
                    NotifyPropertyChanged("TopMessage");
                }
            }
            else
            {
                PickedFile = null;
                PickedFileStream = null;
                AttachmentColorString = "White";
                AttachmentIconString = "\uE898";
                AttachmentPathString = "";
                NotifyPropertyChanged("TopMessage");
            }

            LoadingPopUpIsOpen = false;
        }

        public void ShowPrivateMessages()
        {
            LoadingPopUpIsOpen = true;

            ChannelsVisibility = Visibility.Collapsed;
            GuildSelectedText = "Private Messages";
            ClearGuild();

            var dMChannels = LoginService.client.DMChannels;
            DMChannelsList.Clear();
            foreach (var dmchannel in dMChannels)
                DMChannelsList.Add(dmchannel);
            PrivateMessagesVisibility = Visibility.Visible;
            LoadingPopUpIsOpen = false;
        }

        public async Task SelectDMChannel(object sender, ItemClickEventArgs e)
        {
            LoadingPopUpIsOpen = true;
            if (e != null)
                DMChannel = (SocketDMChannel)e.ClickedItem;
            HasSendMessagePermission = true;
            HasSendFilePermission = true;
            NumOfMessages = 30;
            IEnumerable<IMessage> tempMessageList = await DMChannel.GetMessagesAsync(NumOfMessages).Flatten();
            MessageList.Clear();
            MessageListCopy.Clear();
            foreach (var item in tempMessageList)
            {
                MessageList.Insert(0, item);
                MessageListCopy.Insert(0, item);
            }
            UsersSplitViewPaneOpen = !UsersSplitViewPaneOpen;
            TopMessage = DMChannel.Recipient.Username;
            NoChannelVisibility = Visibility.Visible;
            GroupedGuildUsers = null;
            NotifyPropertyChanged("GroupedGuildUsers");
            if (MessageList.Count > 0)
                NoChannelVisibility = Visibility.Collapsed;
            else
                NoChannelMessage = "No Messages...";
            LoadingPopUpIsOpen = false;
        }

        public void ShowAttachedFlyoutUsersHolding(object sender, HoldingRoutedEventArgs e)
        {
            SocketGuildUser heldUser = (SocketGuildUser)((FrameworkElement)e.OriginalSource).DataContext;
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started && ((ListView)sender).Items.Any())
            {
                MenuFlyout myFlyout = new MenuFlyout();

                MenuFlyoutItem profileOption = new MenuFlyoutItem { Text = "Profile" };
                MenuFlyoutItem mentionOption = new MenuFlyoutItem { Text = "Mention" };

                profileOption.Click += (sender2, e2) =>
                {
                    //something
                };

                mentionOption.Click += (sender2, e2) =>
                {
                    string mention = heldUser.Mention;
                    if (TextBoxMessage == "")
                        TextBoxMessage += mention;
                    else
                        TextBoxMessage += " " + mention;
                };

                myFlyout.Items.Add(profileOption);
                myFlyout.Items.Add(mentionOption);

                if (heldUser.Id != User.Id)
                {
                    MenuFlyoutItem messageOption = new MenuFlyoutItem { Text = "Message" };
                    //MenuFlyoutItem addFriendOption = new MenuFlyoutItem { Text = "Add Friend" };
                    //MenuFlyoutItem blockOption = new MenuFlyoutItem { Text = "Block" };
                    MenuFlyoutSubItem rolesListOption = new MenuFlyoutSubItem { Text = "Roles" };

                    messageOption.Click += (sender2, e2) =>
                    {
                        var task = Task.Run(async () =>
                        {
                            return await heldUser.GetOrCreateDMChannelAsync();
                        });
                        DMChannel = task.Result;
                        SelectDMChannel(null, null);
                        var dMChannels = LoginService.client.DMChannels;
                        DMChannelsList.Clear();
                        foreach (var dmchannel in dMChannels)
                            DMChannelsList.Add(dmchannel);
                        PrivateMessagesVisibility = Visibility.Visible;
                        ChannelsVisibility = Visibility.Collapsed;
                    };

                    //addFriendOption.Click += (sender2, e2) =>
                    //{
                    //   //exists ?
                    //};

                    //blockOption.Click += (sender2, e2) =>
                    //{
                    //    //test
                    //};

                    foreach (SocketRole role in heldUser.Roles.OrderByDescending(x => x.Position).Where(x => !x.IsEveryone))
                    {
                        SolidColorBrush roleColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                        if (role.Color.RawValue != Color.Default.RawValue)
                            roleColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, role.Color.R, role.Color.G, role.Color.B));
                        rolesListOption.Items.Add(new MenuFlyoutItem { Text = role.Name, Foreground = roleColor });
                    }


                    myFlyout.Items.Add(messageOption);
                    //myFlyout.Items.Add(new MenuFlyoutSeparator());
                    //myFlyout.Items.Add(addFriendOption);
                    //myFlyout.Items.Add(blockOption);
                    myFlyout.Items.Add(new MenuFlyoutSeparator());
                    myFlyout.Items.Add(rolesListOption);
                }

                myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
            }
        }

        public void ShowAttachedFlyoutMessagesHolding(object sender, HoldingRoutedEventArgs e)
        {
            IMessage heldMessage = (IMessage)((FrameworkElement)e.OriginalSource).DataContext;
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started && ((ListView)sender).Items.Any())
            {
                MenuFlyout myFlyout = new MenuFlyout();

                MenuFlyoutItem copyOption = new MenuFlyoutItem { Text = "Copy" };
                MenuFlyoutItem deleteOption = new MenuFlyoutItem { Text = "Delete" };

                copyOption.Click += (sender2, e2) =>
                {
                    Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dataPackage.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                    dataPackage.SetText(heldMessage.Content);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                };

                deleteOption.Click += (sender2, e2) =>
                {
                    LoadingPopUpIsOpen = true;
                    heldMessage.DeleteAsync();
                    LoadingPopUpIsOpen = false;
                };

                myFlyout.Items.Add(copyOption);
                if (heldMessage.Author.Username == UserName)
                {
                    myFlyout.Items.Add(deleteOption);
                }

                myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
            }
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
            if (NewGuildName != "" && NewGuildServerLocation != "Select Server")
            {
                foreach (var region in LoginService.client.VoiceRegions)
                {
                    if (region.Name == NewGuildServerLocation)
                    {
                        NewGuildName = NewGuildName.Replace(" ", "_");
                        LoginService.client.CreateGuildAsync(NewGuildName, region);
                        CreateGuildPopUpOpenProperty = false;
                        NewGuildName = "";
                        NewGuildServerLocation = "Select Server";
                    }
                }
            }
        }

        public void CreateChannelPopUpCreate(object sender, RoutedEventArgs e)
        {
            if (NewChannelName != "" && Guild != null)
            {
                LoadingPopUpIsOpen = true;
                NewChannelName = NewChannelName.Replace(" ", "_");
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

        public void GuildSettingsPopUpOpen(object sender, RoutedEventArgs e)
        {
            ChannelsSplitViewPaneOpen = !ChannelsSplitViewPaneOpen;
            GuildSettingsPopUpOpenProperty = true;
        }

        public void GuildSettingsPopUpCancel(object sender, RoutedEventArgs e)
        {
            GuildSettingsPopUpOpenProperty = false;
        }

        public void CreateChannelPopUpCancel(object sender, RoutedEventArgs e)
        {

            CreateChannelPopUpOpenProperty = false;
        }

        public void CreateGuildServerSelect(object sender, RoutedEventArgs e)
        {
            var selection = ((MenuFlyoutItem)sender).Text;
            NewGuildServerLocation = selection;
        }

        public void SetPopUpCenter(object sender, object e)
        {
            ScreenHorizontalCenter = (int)((ApplicationView.GetForCurrentView().VisibleBounds.Width / 2) - (((Grid)sender).ActualWidth / 2));
            ScreenVerticalCenter = (int)((ApplicationView.GetForCurrentView().VisibleBounds.Height / 2) - (((Grid)sender).ActualHeight / 2));
        }

        //######################################################################

        private string attachmentPathString = "";

        public string AttachmentPathString
        {
            get
            {
                return attachmentPathString;
            }
            set
            {
                if (value != attachmentPathString)
                {
                    attachmentPathString = value;
                    NotifyPropertyChanged("AttachmentPathString");
                }
            }
        }

        private string attachmentColorString = "White";

        public string AttachmentColorString
        {
            get
            {
                return attachmentColorString;
            }
            set
            {
                if (value != attachmentColorString)
                {
                    attachmentColorString = value;
                    NotifyPropertyChanged("AttachmentColorString");
                }
            }
        }

        private string attachmentIconString = "\uE898";

        public string AttachmentIconString
        {
            get
            {
                return attachmentIconString;
            }
            set
            {
                if (value != attachmentIconString)
                {
                    attachmentIconString = value;
                    NotifyPropertyChanged("AttachmentIconString");
                }
            }
        }


        public ObservableCollection<IDMChannel> DMChannelsList = new ObservableCollection<IDMChannel>();

        public IOrderedEnumerable<IGrouping<SocketRole, SocketGuildUser>> GroupedGuildUsers;
        public ObservableCollection<SocketGuild> GuildsList = new ObservableCollection<SocketGuild>();

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

        public bool LoadingDisableMenu = false;
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
                    LoadingDisableMenu = !value;
                    NotifyPropertyChanged("LoadingPopUpIsOpen");
                    NotifyPropertyChanged("LoadingDisableMenu");
                }
            }
        }

        private Visibility guildSettingsButtonVisibility = Visibility.Collapsed;

        public Visibility GuildSettingsButtonVisibility
        {
            get
            {
                return guildSettingsButtonVisibility;
            }
            set
            {
                if (value != guildSettingsButtonVisibility)
                {
                    guildSettingsButtonVisibility = value;
                    NotifyPropertyChanged("GuildSettingsButtonVisibility");
                }
            }
        }

        private Visibility privateMessagesVisibility = Visibility.Collapsed;

        public Visibility PrivateMessagesVisibility
        {
            get
            {
                return privateMessagesVisibility;
            }
            set
            {
                if (value != privateMessagesVisibility)
                {
                    privateMessagesVisibility = value;
                    NotifyPropertyChanged("PrivateMessagesVisibility");
                }
            }
        }

        private Visibility channelsVisibility = Visibility.Visible;

        public Visibility ChannelsVisibility
        {
            get
            {
                return channelsVisibility;
            }
            set
            {
                if (value != channelsVisibility)
                {
                    channelsVisibility = value;
                    NotifyPropertyChanged("ChannelsVisibility");
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

        private bool hasSendFilePermission = false;

        public bool HasSendFilePermission
        {
            get
            {
                return hasSendFilePermission;
            }
            set
            {
                if (value != hasSendFilePermission)
                {
                    hasSendFilePermission = value;
                    NotifyPropertyChanged("HasSendFilePermission");
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

        private string guildSettingsLeaveDeleteText = "Leave Guild";

        public string GuildSettingsLeaveDeleteText
        {
            get
            {
                return guildSettingsLeaveDeleteText;
            }
            set
            {
                if (value != guildSettingsLeaveDeleteText)
                {
                    guildSettingsLeaveDeleteText = value;
                    NotifyPropertyChanged("GuildSettingsLeaveDeleteText");
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

        private string newGuildServerLocation = "Select Server";

        public string NewGuildServerLocation
        {
            get
            {
                return newGuildServerLocation;
            }
            set
            {
                if (value != newGuildServerLocation)
                {
                    newGuildServerLocation = value;
                    NotifyPropertyChanged("NewGuildServerLocation");
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

        private bool guildSettingsPopUpOpenProperty = false;

        public bool GuildSettingsPopUpOpenProperty
        {
            get
            {
                return guildSettingsPopUpOpenProperty;
            }
            set
            {
                if (value != guildSettingsPopUpOpenProperty)
                {
                    guildSettingsPopUpOpenProperty = value;
                    NotifyPropertyChanged("GuildSettingsPopUpOpenProperty");
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
        private string textBoxMessage = "";

        public string TextBoxMessage
        {
            get
            {
                return textBoxMessage;
            }
            set
            {
                if (value != textBoxMessage)
                {
                    textBoxMessage = value;
                    NotifyPropertyChanged("TextBoxMessage");
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

        public static Collection<SocketGuildUser> GuildUserList = new Collection<SocketGuildUser>();

        public ObservableCollection<SocketGuildChannel> TextChannelsList = new ObservableCollection<SocketGuildChannel>();
        public ObservableCollection<SocketGuildChannel> VoiceChannelsList = new ObservableCollection<SocketGuildChannel>();

        private string usersTyping = "";

        public string UsersTypingString
        {
            get
            {
                return usersTyping;
            }
            set
            {
                if (value != usersTyping)
                {
                    usersTyping = value;
                    NotifyPropertyChanged("UsersTypingString");
                }
            }
        }

        private bool usersTypingVisibility = false;

        public bool UsersTypingVisibility
        {
            get
            {
                return usersTypingVisibility;
            }
            set
            {
                if (value != usersTypingVisibility)
                {
                    usersTypingVisibility = value;
                    NotifyPropertyChanged("UsersTypingVisibility");
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

        public ObservableCollection<IMessage> PinnedMessageList = new ObservableCollection<IMessage>();
        public ObservableCollection<IMessage> MessageList = new ObservableCollection<IMessage>();
        public static Collection<IMessage> MessageListCopy = new Collection<IMessage>();

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