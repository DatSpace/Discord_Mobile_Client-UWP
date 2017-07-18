using Discord;
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
            if (value != null && value.ToString().Length >= 20)
            {
                return string.Format(value.ToString().Substring(0, 19) + "...");
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
            if (value.ToString() == "Discord Mobile Client")
            {
                return "Please select a channel...";
            }
            else if (ChatViewModel.PickedFile != null)
            {
                return "Add a comment! (Optional)";
            }
            else
            {
                return string.Format("Message #" + value.ToString());
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
            string messageContent = null;
            if ((IMessage)value != null)
            {
                messageContent = ((IMessage)value).Content;
                IReadOnlyCollection<ulong> MentionedUserIdList = ((IMessage)value).MentionedUserIds;
                IReadOnlyCollection<ulong> MentionedRoleIdList = ((IMessage)value).MentionedRoleIds;
                IReadOnlyCollection<ulong> MentionedChannelIdList = ((IMessage)value).MentionedChannelIds;

                foreach (ulong mentioneduserid in MentionedUserIdList)
                {
                    if (messageContent.Contains("<@" + mentioneduserid.ToString() + ">"))
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
                    if (messageContent.Contains("<@&" + mentionedroleid.ToString() + ">"))
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
                    if (messageContent.Contains("<#" + mentionedchannelid.ToString() + ">"))
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
            }
            return messageContent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
