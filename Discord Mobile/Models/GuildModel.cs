using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Mobile.Services;
using Discord.WebSocket;

namespace Discord_Mobile.Models
{
    class GuildModel
    {
        public IReadOnlyCollection<SocketGuild> GetGuilds()
        {
            return LoginService.client.Guilds;
        }

    }
}
