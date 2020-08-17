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
        public static Server QueryServer(string ip, string rconPwd = null, int rconPort = 0)
        {
            string[] formattedIP = ip.Split(':'); //split IP into IP and port

            //Get server instance 
            QueryMaster.GameServer.Server server = GetServerInstance(ip, rconPwd);
            ServerInfo info = server.GetInfo();

            //If RCON password has been submitted, check to see if it's correct.
            //If not, then set RCON password back to null because it's incorrect
            if (rconPwd != null) if (!server.GetControl(rconPwd)) rconPwd = null;

            return new Server()
            {
                Address = info.Address,
                Map = info.Map,
                MaxPlayers = info.MaxPlayers,
                Name = info.Name,
                Players = info.Players,
                RconPwd = rconPwd,
                RconPort = rconPort
            };
        }

        public static Player FindPlayer(string player)
        {
            throw new NotImplementedException("Not yet implemented!");
        }

        public static QueryMaster.GameServer.Server GetServerInstance(string ip, string rconPwd = null)
        {
            string[] formattedIP = ip.Split(':');
            var ipEndpoint = new IPEndPoint(IPAddress.Parse(formattedIP[0]), int.Parse(formattedIP[1]));

            //Get server instance
            QueryMaster.GameServer.Server server = ServerQuery.GetServerInstance(Game.Rust, ipEndpoint, sendTimeout: 500, receiveTimeout: 500, throwExceptions: true);

            //If rconPwd is not null, check if we can get control of RCON
            if(rconPwd != null) if(!server.GetControl(rconPwd)) return null;

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
            //TODO: fill this out
        }
    }
}
