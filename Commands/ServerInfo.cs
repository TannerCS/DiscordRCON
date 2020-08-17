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
        public async Task Status(string IP)
        {
            var server = Query.QueryServer(IP);
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(server.Name);
            builder.WithDescription($"IP: {server.Address}\nMap Type: {server.Map}");
            builder.AddField("Players", server.Players, true);
            builder.AddField("Max Players", server.MaxPlayers, true);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("watch"), Alias("add")]
        public async Task AddServer(string IP, string RconPwd = null, int RconPort = 0)
        {
            await Context.Message.DeleteAsync();
            var server = Database.AddServerToWatchlist(Context.Guild.Id, IP, RconPwd, RconPort);
            if (server.Item1)
            {
                await ReplyAsync($"`{server.Item2}` has been added to the watchlist!");
            }
            else
            {
                await ReplyAsync($"`{IP}` could not be added. Maybe it's already added? If not, make sure it's the correct IP and port and try again.");
            }
        }

        [Command("penis"), Alias("add")]
        public async Task Penis(string IP, int RconPort = 0)
        {
            var server = Database.AddServerToWatchlist(Context.Guild.Id, IP, null, RconPort);
            await ReplyAsync("penis");
        }

        [Command("unwatch"), Alias("remove")]
        public async Task RemoveServer(string IP)
        {
            if(Database.RemoveServerFromWatchlist(Context.Guild.Id, IP))
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
            builder.WithTitle("Watchlist");
            var servers = Database.Guilds.First(x => x.GuildID == Context.Guild.Id).Servers.ToArray();
            builder.WithDescription("");

            for (int i = 1; i < servers.Length + 1; i++)
            {
                if (builder.Description.Length < 1984)
                {
                    builder.Description += $"[{i}] {servers[i - 1].Name}";

                    if(servers[i - 1].RconPwd == null) builder.Description += " [NO RCON]";
                    builder.Description += "\n";
                }
            }


            builder.WithFooter("The number in the brackets [ ] is the ID you should use when sending RCON commands to specific servers. IP:PORT can also be used.");

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("updatepwd"), Alias("rconpwd", "pwd")]
        public async Task UpdateRconPassword(string IP, [Remainder]string password)
        {
            await Context.Message.DeleteAsync();
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

            if(IP.Split(':').Length == 2)
            {
                var server = guild.Servers.FirstOrDefault(x => x.Address == IP);
                server.RconPwd = password;
            }
            else if(int.TryParse(IP, out int ID))
            {
                var server = guild.Servers[ID].RconPwd = password;
            }
            else
            {
                await ReplyAsync($"Invalid IP/ID. Refer to `{guild.Prefix}server` for usage of this command.");
            }

            if (Database.UpdateGuild(guild)) await ReplyAsync("Password updated!");
            else await ReplyAsync("Could not update password.");
        }

        [Command("updateport"), Alias("rconport", "port")]
        public async Task UpdateRconPort(string IP, [Remainder]int port)
        {
            await Context.Message.DeleteAsync();
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

            if (int.TryParse(IP, out int ID))
            {
                var server = guild.Servers[ID].RconPort = port;
            }
            else
            {
                await ReplyAsync($"Invalid port. Refer to `{guild.Prefix}server` for usage of this command.");
            }

            if (Database.UpdateGuild(guild)) await ReplyAsync("Port updated!");
            else await ReplyAsync("Could not update port.");
        }

        [Command("playerinfo"), Alias("pinfo", "pi")]
        public async Task PlayerInfo(string IP, [Remainder] string player)
        {
            var guild = Database.Guilds.First(x => x.GuildID == Context.Guild.Id);

            if (IP.Split(':').Length == 2)
            {
                var server = guild.Servers.FirstOrDefault(x => x.Address == IP);
                
            }
            else if (int.TryParse(IP, out int ID))
            {
                var server = guild.Servers[ID];
            }
            else
            {
                await ReplyAsync($"blah blah blah. I'm not implemented yet.");
            }
            await ReplyAsync($"blah blah blah. I'm not implemented yet.");
        }

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
                $"\"{prefix}server playerinfo|pinfo|pi <IP:PORT|Server ID|all> <player name|steamid64>\" - Display basic information about the specified user" +
                $"```");
        }
    }
}
