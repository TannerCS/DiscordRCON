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
        public async Task SendCommand(string ip, [Remainder]string command)
        {
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id); //Get guild
            Query.Server server = null;

            //If ip can be split into IP and PORT
            if(ip.Split(':').Length == 2)
            {
                server = guild.Servers.FirstOrDefault(x => x.Address == ip);
            }
            //If ip is a server ID
            else if(int.TryParse(ip, out int id))
            {
                server = guild.Servers[id - 1];
            }
            //If ip is all, meaning go through ALL servers
            else if(ip.ToLower() == "all")
            {
                foreach(var serv in guild.Servers)
                {
                    if (serv.RconPwd == null) continue; //If server doesn't have an RCON password, skip it

                    //Get srever instance and send the command
                    var serverInstance = Query.GetServerInstance(serv.Address, serv.RconPwd);
                    serverInstance.Rcon.SendCommand(command);
                }

                await ReplyAsync("Sent command to all servers with RCON enabled!");
                return;
            }

            //If RCON password is null, then we can't send a command
            if (server.RconPwd == null)
            {
                await ReplyAsync($"RCON is not available for this server. Try updating your RCON password (hint: {guild.Prefix}server).");
                return;
            }

            var instance = Query.GetServerInstance(server.Address, server.RconPwd); //Get server instance with RCON enabled
            if (instance == null) //If instance is null, then we couldn't successfully get an RCON instance
            {
                await ReplyAsync($"RCON is not available for this server. Try updating your RCON password (hint: {guild.Prefix}server).");
                return;
            }

            //Send command and echo back response
            string res = instance.Rcon.SendCommand(command);
            await ReplyAsync(res);
        }

        [Command]
        public async Task Info()
        {
            var prefix = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Prefix; //Get prefix

            await ReplyAsync("```json\n" +
                $"\"{prefix}rcon send <IP:PORT|Server ID|all> <command>\" - Sends a custom user-specified command" +
                $"```");
        }
    }
}
