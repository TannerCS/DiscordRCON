using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRCON.Commands
{
    public class Prefix : ModuleBase<SocketCommandContext>
    {
        [Command("prefix")]
        public async Task ChangePrefix(string Prefix = "")
        {
            if (Context.User.Id != Context.Guild.OwnerId) return;

            if(Prefix == "")
            {
                var prefix = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Prefix;
                await ReplyAsync($"Your current prefix is: {prefix}\nTo change the prefix, type {prefix}prefix <new prefix>");
                return;
            }

            Database.ChangePrefix(Context.Guild.Id, Prefix);

            await ReplyAsync($"Guild prefix changed to {Prefix}");
        }
    }
}
