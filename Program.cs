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
        private DiscordSocketClient _client;
        private CommandService _commands;

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });
            _commands = new CommandService();
            new Database();

            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;
            _client.JoinedGuild += JoinedGuild;
            _client.Ready += Ready;

            await _client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await _client.StartAsync();

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            foreach (var guild in Database.Guilds)
            {
                foreach (var server in guild.Servers)
                {
                    if (server.RconPwd == null) continue;

                    var instance = Query.GetServerInstance(server.Address, server.RconPwd);

                    if (instance == null) continue;

                    if (server.RconPort == 0) continue; 
                    
                    instance.Rcon.Enablelogging();

                    var logs = instance.GetLogs(server.RconPort);
                    logs.Start();
                    logs.Callback += Callback;
                    await Log(new LogMessage(LogSeverity.Info, "Logs", $"Started logging for {server.Name}"));
                }
            }
        }

        public static void Callback(string log)
        {
            Console.WriteLine($"[LOG] {log}");
        }

        private async Task JoinedGuild(SocketGuild arg)
        {
            if (!Database.CreateNewGuild(arg.Id)) await Log(new LogMessage(LogSeverity.Critical, "Database", "Could not create new guild in Database"));
            else await Log(new LogMessage(LogSeverity.Debug, "Database", $"Created new Guild entry: {arg.Name}"));

            await arg.Owner.SendMessageAsync($"Hello there! Thanks for inviting me to your server. " +
                $"I'm a bot, obviously, made to make *your* life easier. Now, while chatting with your friends on Discord, you can kick and ban them without even switching windows! " +
                $"Only you are allowed to use these commands (for now). I'm in *very* early stages of development, so please be patient with me. Sometimes things may not work on the first try. " +
                $"If they don't, send me a message with a screenshot and a guide on how I can reproduce this error. Enjoy! Tanner#5116");
        }

        public static async Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            int argPos = 0;

            if (!(messageParam is SocketUserMessage message) || message.Author.IsBot || !(message.HasStringPrefix(Database.GetGuildPrefix((message.Channel as IGuildChannel).GuildId), ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            if (message.Channel is IDMChannel)
            {
                await message.Channel.SendMessageAsync("I don't support DM commands! Try me in a private channel in a Guild!");
                return;
            }

            var context = new SocketCommandContext(_client, message);
            
            if(message.Author.Id != (message.Channel as IGuildChannel).Guild.OwnerId)
            {
                await context.Channel.SendMessageAsync($"<@{message.Author.Id}> Sorry, you can't use me!");
                return;
            }

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
