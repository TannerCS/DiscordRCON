using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRCON.Commands
{
    [Group("rcon")]
    public class RCON : ModuleBase<SocketCommandContext>
    {
        [Command("send")]
        public async Task SendCommand(string server, [Remainder]string command)
        {
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);
            Query.Server s = null;
            if(server.Split(':').Length == 2)
            {
                s = guild.Servers.FirstOrDefault(x => x.Address == server);
            }
            else if(int.TryParse(server, out int ID))
            {
                s = guild.Servers[ID];
            }
            else if(server.ToLower() == "all")
            {
                foreach(var serv in guild.Servers)
                {
                    if (serv.RconPwd == "") continue;
                    var inst = Query.GetServerInstance(serv.Address, serv.RconPwd);
                    inst.Rcon.SendCommand(command);
                }

                await ReplyAsync("Sent command to all servers with RCON enabled!");
            }

            if (string.IsNullOrWhiteSpace(s.RconPwd))
            {
                await ReplyAsync($"RCON is not available for this server. Try updating your RCON password (hint: {guild.Prefix}server).");
                return;
            }

            var instance = Query.GetServerInstance(s.Address, s.RconPwd);

            if (instance == null)
            {
                await ReplyAsync($"RCON is not available for this server. Try updating your RCON password (hint: {guild.Prefix}server).");
                return;
            }

            string res = instance.Rcon.SendCommand(command);
            await ReplyAsync(res);
        }

        [Command]
        public async Task Info()
        {
            var prefix = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Prefix;
            await ReplyAsync("```json\n" +
                $"\"{prefix}rcon send <IP:PORT|Server ID|all> <command>\" - Sends a custom user-specified command" +
                $"```");
        }
    }
}
