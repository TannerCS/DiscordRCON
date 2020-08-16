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
            Db = new LiteDatabase("database.db");
            //Todo: Add existing guilds from database to Guilds list
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                Guilds = col.Query().ToList();

                Program.Log(new LogMessage(LogSeverity.Info, "Database", $"Database Loaded Successfully. {Guilds.Count} guilds loaded."));
            }
            catch(Exception e)
            {

            }
            //Todo: Add existing Rust servers from database to their guild
        }

        public static bool CreateNewGuild(ulong GuildID)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                var guild = new Guild()
                {
                    GuildID = GuildID,
                    Prefix = "!",
                    Servers = new List<Query.Server>()
                };
                col.Insert(guild);

                Guilds.Add(guild);

                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        public static bool UpdateGuild(Guild guild)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                col.Update(guild);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool ChangePrefix(ulong GuildID, string Prefix)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                var guild = Guilds.First(x => x.GuildID == GuildID);
                guild.Prefix = Prefix;
                col.Update(guild);
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        public static string GetGuildPrefix(ulong GuildID)
        {
            return Guilds.First(x => x.GuildID == GuildID).Prefix;
        }

        public static Tuple<bool, string> AddServerToWatchlist(ulong GuildID, string IP, string RconPwd)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                var guild = col.FindOne(x => x.GuildID == GuildID);
                var server = Query.QueryServer(IP, RconPwd);

                if (guild.Servers.FirstOrDefault(x => x.Address == IP) == null) guild.Servers.Add(server);
                else return new Tuple<bool, string>(false, "");

                col.Update(guild);
                Guilds.Find(x => x.GuildID == guild.GuildID).Servers.Add(server);

                return new Tuple<bool, string>(true, server.Name);
            }catch(Exception e)
            {
                return new Tuple<bool, string>(false, "");
            }
        }

        public static bool RemoveServerFromWatchlist(ulong guildID, string IP)
        {
            try
            {
                var col = Db.GetCollection<Guild>("guilds");
                var guild = col.FindOne(x => x.GuildID == guildID);

                if(IP.Split(':').Length == 2)
                {
                    var s = guild.Servers.FirstOrDefault(x => x.Address == IP);

                    if (s == null)
                    {
                        return false;
                    }

                    guild.Servers.Remove(s);
                    col.Update(guild);
                    Guilds.Find(x => x.GuildID == guild.GuildID).Servers.Remove(s);

                    return true;
                }

                int ID = int.Parse(IP);
                var server = guild.Servers.ElementAtOrDefault(ID);

                if (server == null) return false;

                guild.Servers.Remove(server);
                col.Update(guild);
                Guilds.Find(x => x.GuildID == guild.GuildID).Servers.Remove(server);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public class Guild
        {
            public ulong GuildID { get; set; }
            public string Prefix { get; set; }
            public List<Query.Server> Servers { get; set; }
        }
    }
}
