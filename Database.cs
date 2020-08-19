using Discord;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DiscordRCON
{
    public class Database
    {
        public static LiteDatabase Db;

        public static List<Guild> Guilds;
        public Database()
        {
            try
            {
                Db = new LiteDatabase("database.db");
                var col = Db.GetCollection<Guild>("guilds"); //Get updated version of the guilds
                Guilds = col.Query().ToList(); //Push updated guilds
            }
            catch (Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "Database", $"Could not load database!\n {e}"));
            }
        }

        public static bool CreateNewGuild(ulong guildID)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds"); //Get updated version of the guilds
                var guild = new Guild()
                {
                    GuildID = guildID,
                    Prefix = "!",
                    Servers = new List<Query.Server>()
                };

                col.Insert(guild); //Update database
                Guilds.Add(guild); //Update memory

                return true; //Function returns successfully
            }catch(Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "CreateNewGuild", $"Could not create new guild! ({guildID})\n {e}"));
                return false;
            }
        }

        public static bool ChangePrefix(ulong guildID, string prefix)
        {
            try
            {
                var guild = Guilds.First(x => x.GuildID == guildID); //Get guild. Not using FirstOrDefault, because the guildID is guaranteed to be listed
                guild.Prefix = prefix; //Change prefix to new prefix

                var col = Db.GetCollection<Guild>("guilds");
                col.Update(guild);

                return true;
            }catch(Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "ChangePrefix", $"Could not change prefix! ({guildID}, {prefix})\n {e}"));
                return false;
            }
        }

        public static string GetGuildPrefix(ulong guildID)
        {
            return Guilds.First(x => x.GuildID == guildID).Prefix; //Get guild's prefix. Not using FirstOrDefault because the guildID is guaranteed to be listed
        }

        public static Tuple<bool, string> AddServerToWatchlist(ulong guildID, string ip, string rconPwd, int rconPort)
        {
            try
            {
                //get guild
                var col = Db.GetCollection<Guild>("guilds");
                var guild = col.FindOne(x => x.GuildID == guildID);

                var server = Query.QueryServer(ip, rconPwd, rconPort); //Query server for information

                if (guild.Servers.FirstOrDefault(x => x.Address == ip) == null) guild.Servers.Add(server);
                else return new Tuple<bool, string>(false, ""); //return false and no name because it didn't return successfully

                col.Update(guild); //Update database
                Guilds.Find(x => x.GuildID == guild.GuildID).Servers.Add(server); //Updata memory

                if(rconPwd != null)
                {
                    var instance = Query.GetServerInstance(server.Address, server.RconPwd);

                    if (instance == null) return new Tuple<bool, string>(false, ""); //return false and no name because it didn't return successfully
                    if (server.RconPort == 0) return new Tuple<bool, string>(false, ""); //return false and no name because it didn't return successfully

                    //Enable logging for this server if the RCON password and RCON port are correct
                    instance.Rcon.Enablelogging();
                    var logs = instance.GetLogs(server.RconPort);
                    logs.Start();
                    logs.Callback += Program.Callback;

                    Logger.Log(new LogMessage(LogSeverity.Info, "AddServerToWatchlist", $"Started logging for {server.Name}"));
                }

                return new Tuple<bool, string>(true, server.Name);
            }catch(Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "AddServerToWatchlist", $"Could not add server to watchlist ({guildID}, {ip}\n {e}"));
                return new Tuple<bool, string>(false, "");
            }
        }

        public static bool UpdateGuild(Guild guild)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                col.Update(guild);
                return true;
            }catch(Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "UpdateGuild", $"Could not update guild ({guild.GuildID})\n {e}"));
                return false;
            }
        }

        public static bool RemoveServerFromWatchlist(ulong guildID, string ip)
        {
            try
            {
                //Get guild
                var col = Db.GetCollection<Guild>("guilds");
                var guild = col.FindOne(x => x.GuildID == guildID);

                if (ip.Split(':').Length == 2) //If ip is an actual IP. 127.0.0.1:00000
                {
                    var server = guild.Servers.FirstOrDefault(x => x.Address == ip); //Get server

                    if (server == null) //If server is null, then it doesn't exist in the list
                    {
                        return false;
                    }

                    //Remove server, update database and memory
                    guild.Servers.Remove(server);
                    col.Update(guild);
                    Guilds.Find(x => x.GuildID == guild.GuildID).Servers.Remove(server);

                    return true;
                }
                else if (int.TryParse(ip, out int id)) //Else, try to parse the ip and see if it's a server ID
                {
                    id -= 1; //Account for neat formatting for server watchlist (since it starts at 1)

                    var server = guild.Servers.ElementAtOrDefault(id); //get Server at that id

                    if (server == null) return false; //If server is null, then the ID is either too big or less than 1

                    //Remove server, update database, and remove server from memory
                    guild.Servers.Remove(server);
                    col.Update(guild);
                    Guilds.Find(x => x.GuildID == guild.GuildID).Servers.RemoveAt(id);
                    return true;
                }
                //Else, return false if we can't do anything with it
                else return false;
            }
            catch (Exception e)
            {
                Logger.Log(new LogMessage(LogSeverity.Critical, "RemoveServerFromWatchlist", $"Could not remove server from watchlist ({guildID}, {ip}\n {e}"));
                return false;
            }
        }
    }

    public class Guild
        {
            public ulong GuildID { get; set; }
            public string Prefix { get; set; }
            public List<Query.Server> Servers { get; set; }
        }
}
