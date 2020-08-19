using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordRCON
{
    class Program
    {
        private DiscordSocketClient _Client;
        private CommandService _Commands;

        public static void Main() => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            _Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });
            _Commands = new CommandService();
            new Database();

            _Client.Log += Logger.LogAsync;
            _Client.MessageReceived += HandleCommandAsync;
            _Client.JoinedGuild += JoinedGuild;
            _Client.Ready += Ready;

            await _Client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await _Client.StartAsync();

            await _Commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            //Loop through each guild while looping through each server to start their logging service
            foreach (var guild in Database.Guilds)
            {
                foreach (var server in guild.Servers)
                {
                    
                    if (server.RconPwd == null) continue; //If server doesn't have an rcon password, skip it

                    //Get server instance so we can grab RCON logs
                    var instance = Query.GetServerInstance(server.Address, server.RconPwd);
                    if (instance == null) continue;

                    if (server.RconPort == 0) continue; //If server doesn't have an RCON port, we can't connect to it

                    //Enable logging for server and start logging
                    instance.Rcon.Enablelogging();
                    var logs = instance.GetLogs(server.RconPort);
                    logs.Start();
                    logs.Callback += Callback;

                    await Logger.LogAsync(new LogMessage(LogSeverity.Info, "Logs", $"Started logging for {server.Name}"));
                }
            }
        }

        public static void Callback(string log)
        {
            //TODO: Flesh this out with ServerInfo.cs !server watchflags
            Console.WriteLine($"[LOG] {log}");
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            //If CreateNewGuild returns FALSE (can't create new guild), log message. If it returns true (else), then it created the new guild successfully.
            if (!Database.CreateNewGuild(guild.Id)) await Logger.LogAsync(new LogMessage(LogSeverity.Critical, "Database", "Could not create new guild in Database"));
            else await Logger.LogAsync(new LogMessage(LogSeverity.Debug, "Database", $"Created new Guild entry: {guild.Name}"));

            await guild.Owner.SendMessageAsync($"Hello there! Thanks for inviting me to your server. " +
                $"I'm a bot, obviously, made to make *your* life easier. Now, while chatting with your friends on Discord, you can kick and ban them without even switching windows! " +
                $"Only you are allowed to use these commands (for now). I'm in *very* early stages of development, so please be patient with me. Sometimes things may not work on the first try. " +
                $"If they don't, send me a message with a screenshot and a guide on how I can reproduce this error. Enjoy! Tanner#5116");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            int argPos = 0;

            //Check if message is from a user, is not a bot, and has the guild's prefix
            if (!(messageParam is SocketUserMessage message) || message.Author.IsBot || !(message.HasStringPrefix(Database.GetGuildPrefix((message.Channel as IGuildChannel).GuildId), ref argPos) ||
                message.HasMentionPrefix(_Client.CurrentUser, ref argPos))) return;

            //Disable DM messages because I'm lazy and don't want to deal with them. Maybe later. TODO
            if (message.Channel is IDMChannel)
            {
                await message.Channel.SendMessageAsync("I don't support DM commands! Try me in a private channel in a Guild!");
                return;
            }

            var context = new SocketCommandContext(_Client, message);
            
            //TODO: Allow people to whitelist roles and people
            if(message.Author.Id != (message.Channel as IGuildChannel).Guild.OwnerId)
            {
                await context.Channel.SendMessageAsync($"<@{message.Author.Id}> Sorry, you can't use me!");
                return;
            }

            await _Commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
