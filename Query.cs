using QueryMaster;
using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace DiscordRCON
{
    public class Query
    {
        public static Server QueryServer(string IP, string RconPwd = null, int RconPort = 0)
        {
            string[] formattedIP = IP.Split(':');
            QueryMaster.GameServer.Server server = ServerQuery.GetServerInstance(Game.Rust, new IPEndPoint(IPAddress.Parse(formattedIP[0]), int.Parse(formattedIP[1])), sendTimeout: 500, receiveTimeout: 500);
            ServerInfo info = server.GetInfo();

            if(RconPwd != null) if (!server.GetControl(RconPwd)) RconPwd = null;

            return new Server()
            {
                Address = info.Address,
                Map = info.Map,
                MaxPlayers = info.MaxPlayers,
                Name = info.Name,
                Players = info.Players,
                RconPwd = RconPwd,
                RconPort = RconPort
            };
        }

        public static Player FindPlayer(string player)
        {
            return new Player();
        }

        public static QueryMaster.GameServer.Server GetServerInstance(string IP, string RconPwd)
        {
            string[] formattedIP = IP.Split(':');
            QueryMaster.GameServer.Server server = ServerQuery.GetServerInstance(Game.Rust, new IPEndPoint(IPAddress.Parse(formattedIP[0]), int.Parse(formattedIP[1])), throwExceptions: true);

            if (!server.GetControl(RconPwd)) return null;

            return server;
        }

        public class Server
        {
            public string Address { get; set; }
            public string Name { get; set; }
            public string Map { get; set; }
            public int Players { get; set; }
            public int MaxPlayers { get; set; }
            public string RconPwd { get; set; }
            public int RconPort { get; set; }
        }

        public class Player
        {

        }
    }
}
