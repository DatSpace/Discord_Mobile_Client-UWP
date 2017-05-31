using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace Discord_Mobile.Converters
{
    class ChannelNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value.ToString().Length >= 18)
            {
                return string.Format(value.ToString().Substring(0, 17) + "...");
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class UserNameToNicknameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                string usernickname;

                if (value is RestUser)
                    usernickname = ((RestUser)value).Username;
                else
                {
                    usernickname = ((SocketGuildUser)value).Nickname;
                    if (usernickname == null || usernickname == "")
                    {
                        int temp = value.ToString().LastIndexOf('#');
                        return value.ToString().Substring(0, temp);
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

    //class UserTypingConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value != null)
    //        {
    //            if (value.ToString().Length > 20)
    //            {
    //                while (value.ToString().Length > 20)
    //                {
    //                    int tempindex;
    //                    tempindex = value.ToString().LastIndexOf(",");
    //                    value.ToString().Substring(0, tempindex);
    //                }
    //                value += " and more are typing...";
    //            }
    //            else if (!value.ToString().Contains(","))
    //                value += " is typing...";
    //            else
    //                value += " are typing...";
    //        }
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    class ChannelTopTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value.ToString() != "Discord Mobile Client")
            {
                if (value.ToString().Length >= 25)
                {
                    return string.Format("#" + value.ToString().Substring(0, 24) + "...");
                }
                else
                {
                    return string.Format("#" + value.ToString());
                }
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class PlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return string.Format("Message #" + value.ToString());
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class MessageMentionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string messageContent = (string)value;
            foreach (IMessage message in ChatViewModel.MessageListCopy)
            {
                if (message.Content == (string)value)
                {
                    IReadOnlyCollection<ulong> MentionedUserIdList = message.MentionedUserIds;
                    IReadOnlyCollection<ulong> MentionedRoleIdList = message.MentionedRoleIds;
                    IReadOnlyCollection<ulong> MentionedChannelIdList = message.MentionedChannelIds;

                    foreach (ulong mentioneduserid in MentionedUserIdList)
                    {
                        if (((string)value).Contains("<@" + mentioneduserid.ToString() + ">"))
                        {
                            string usernameOrNickname = "";
                            foreach (SocketGuildUser user in ChatViewModel.GuildUserList)
                            {
                                if (user.Id == mentioneduserid)
                                {
                                    if (user.Nickname != null)
                                        usernameOrNickname = user.Nickname;
                                    else
                                        usernameOrNickname = user.Username;
                                    break;
                                }

                            }

                            messageContent = messageContent.Replace("<@" + mentioneduserid.ToString() + ">", "@" + usernameOrNickname);
                        }
                    }
                    foreach (ulong mentionedroleid in MentionedRoleIdList)
                    {
                        if (((string)value).Contains("<@&" + mentionedroleid.ToString() + ">"))
                        {
                            string rolename = "";
                            foreach (var role in ChatViewModel.Guild.Roles)
                            {
                                if (role.Id == mentionedroleid)
                                {
                                    rolename = role.Name;
                                    break;
                                }
                            }

                            messageContent = messageContent.Replace("<@&" + mentionedroleid.ToString() + ">", "@" + rolename);
                        }
                    }
                    foreach (ulong mentionedchannelid in MentionedChannelIdList)
                    {
                        if (((string)value).Contains("<#" + mentionedchannelid.ToString() + ">"))
                        {
                            string channelname = "";
                            foreach (SocketGuildChannel channel in ChatViewModel.Guild.Channels)
                            {
                                if (channel.Id == mentionedchannelid)
                                {
                                    channelname = channel.Name;
                                    break;
                                }
                            }

                            messageContent = messageContent.Replace("<#" + mentionedchannelid.ToString() + ">", "#" + channelname);
                        }
                    }
                    break;
                }
            }
            return messageContent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
