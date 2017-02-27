﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord_Mobile.Services;
using Discord;

namespace Discord_Mobile.Models
{
    class UserModel
    {
        //private LoginService login = new LoginService();
        //private DiscordSocketClient client = LoginService.client;


        public string GetAvatar()
        {
            return LoginService.client.CurrentUser.GetAvatarUrl();
        }
        public string GetName()
        {
            return LoginService.client.CurrentUser.Username;
        }
        public UserStatus GetStatus()
        {
            return LoginService.client.CurrentUser.Status;
        }
    }
}