using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRCON.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpInfo()
        {
            var prefix = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Prefix;
            await ReplyAsync("```json\n" +
                $"Command Usage Guide\n" +
                $"Example command: \"!server add <IP:PORT>|<Server ID> <RCON Password (Optional)>\"\n" +
                $"- \"<>\" means a field is required (unless it has an \"(optional)\" flag)\n" +
                $"- \"|\" means \"or\". It can take an IP and port \"or\" a server ID\n" +
                $"------------------------------------------------------\n" +
                $"\"{prefix}server\" - Shows the help dialogue for server related information\n" +
                $"\"{prefix}rcon\" - Shows the help dialogue for RCON related information\n" +
                $"\"{prefix}prefix <new prefix>\" - Change command prefix" +
                $"```");
        }
    }
}
