using Discord;
using Discord.Commands;
using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRCON.Commands
{
    [Group("server")]
    public class ServerInfo : ModuleBase<SocketCommandContext>
    {
        [Command("status")]
        public async Task Status(string ip)
        {
            //Get server information
            var server = Query.QueryServer(ip);
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(server.Name);
            builder.WithDescription($"IP: {server.Address}\nMap Type: {server.Map}");
            builder.AddField("Players", server.Players, true);
            builder.AddField("Max Players", server.MaxPlayers, true);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("watch"), Alias("add")]
        public async Task AddServer(string ip, string rconPwd = null, int rconPort = 0)
        {
            try
            {
                await Context.Message.DeleteAsync(); //Delete message... For security reasons
            }
            catch (Exception) { }

            var server = Database.AddServerToWatchlist(Context.Guild.Id, ip, rconPwd, rconPort); //Add server to watchlist

            //If returned true, then it was successfully added
            if (server.Item1)
            {
                await ReplyAsync($"`{server.Item2}` has been added to the watchlist!");
            }
            else
            {
                await ReplyAsync($"`{ip}` could not be added. Maybe it's already added? If not, make sure it's the correct IP and port and try again.");
            }
        }

        [Command("unwatch"), Alias("remove")]
        public async Task RemoveServer(string ip)
        {
            //If returned true, then it was successfully removed
            if(Database.RemoveServerFromWatchlist(Context.Guild.Id, ip))
            {
                await ReplyAsync("Removed server from watchlist!");
            }
            else
            {
                await ReplyAsync("Could not remove server. Are you sure you have this server in the watchlist? If not, make sure it's the correct IP and port (or ID) and try again.");
            }
        }

        [Command("watchlist"), Alias("wl")]
        public async Task Watchlist()
        {
            EmbedBuilder builder = new EmbedBuilder();
            var servers = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Servers.ToArray();

            builder.WithTitle("Watchlist");
            builder.WithDescription("");

            //Loop through each server
            for (int i = 1; i < servers.Length + 1; i++)
            {
                //Just to make sure we don't go over the character limit
                if (builder.Description.Length < 1984)
                {
                    builder.Description += $"[{i}] {servers[i - 1].Name}";

                    //Show RCON is disabled for the server if there's no password
                    if(servers[i - 1].RconPwd == null) builder.Description += " [NO RCON]";
                    builder.Description += "\n";
                }
            }

            builder.WithFooter("The number in the brackets [ ] is the ID you should use when sending RCON commands to specific servers. IP:PORT can also be used.");

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("updatepwd"), Alias("rconpwd", "pwd")]
        public async Task UpdateRconPassword(string ip, [Remainder]string password)
        {
            try
            {
                await Context.Message.DeleteAsync(); //Delete message... For security reasons
            }
            catch (Exception) { }

            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

            //Check if it can be split as an IP:PORT
            if(ip.Split(':').Length == 2)
            {
                //Update password
                var server = guild.Servers.FirstOrDefault(x => x.Address == ip);
                server.RconPwd = password;
            }
            //If it's not an IP, try to parse it as an ID
            else if(int.TryParse(ip, out int id))
            {
                var server = guild.Servers[id].RconPwd = password;
            }
            //Else, it can't be used
            else
            {
                await ReplyAsync($"Invalid IP/ID. Refer to `{guild.Prefix}server` for usage of this command.");
                return;
            }

            if (Database.UpdateGuild(guild)) await ReplyAsync("Password updated!");
            else await ReplyAsync("Could not update password.");
        }

        [Command("updateport"), Alias("rconport", "port")]
        public async Task UpdateRconPort(string ip, [Remainder]int port)
        {
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

            //Check if it can be split as an IP:PORT
            if (ip.Split(':').Length == 2)
            {
                //Update port
                var server = guild.Servers.FirstOrDefault(x => x.Address == ip);
                server.RconPort = port;
            }
            //If it's not an IP, try to parse it as an ID
            else if (int.TryParse(ip, out int id))
            {
                var server = guild.Servers[id].RconPort = port;
            }
            //Else, it can't be used
            else
            {
                await ReplyAsync($"Invalid port. Refer to `{guild.Prefix}server` for usage of this command.");
            }

            if (Database.UpdateGuild(guild)) await ReplyAsync("Port updated!");
            else await ReplyAsync("Could not update port.");
        }

        //[Command("playerinfo"), Alias("pinfo", "pi")]
        //public async Task PlayerInfo(string IP, [Remainder] string player)
        //{
        //    var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

        //    if (IP.Split(':').Length == 2)
        //    {
        //        var server = guild.Servers.FirstOrDefault(x => x.Address == IP);

        //    }
        //    else if (int.TryParse(IP, out int ID))
        //    {
        //        var server = guild.Servers[ID];
        //    }
        //    else
        //    {
        //        await ReplyAsync($"blah blah blah. I'm not implemented yet.");
        //    }
        //    await ReplyAsync($"blah blah blah. I'm not implemented yet.");
        //}

        [Command]
        public async Task Info()
        {
            var prefix = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Prefix;
            /*
             *          TODO
             * !server watchflags add <pdeath|shutdown|startup|pban|pkick|pchat> <IP:PORT|Server ID|all> - Add logging flags to customize exactly what you want recieved.
             * 
             * 
             */
            await ReplyAsync($"```json\n" +
                $"\"{prefix}server status <IP:PORT>\" - returns information about a specific server.\n" +
                $"\"{prefix}server watch|add <IP:PORT> <rconPwd (optional)> <RCON port (optional)>\" - Add server to watchlist\n" +
                $"\"{prefix}server unwatch|remove <IP:PORT|Server ID>\" - Remove server from watchlist\n" +
                $"\"{prefix}server watchlist|wl\" - View server watchlist\n" +
                $"\"{prefix}server updatepwd|rconpwd|pwd <IP:PORT|Server ID> <RCON Password>\" - Update RCON password\n" +
                $"\"{prefix}server updateport|rconport|port <IP:PORT|Server ID> <RCON Port>\" - Update RCON port\n" +
                //$"\"{prefix}server playerinfo|pinfo|pi <IP:PORT|Server ID|all> <player name|steamid64>\" - Display basic information about the specified user" +
                $"```");
        }
    }
}
