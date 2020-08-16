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

            await _client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await _client.StartAsync();

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            await Task.Delay(-1);
        }

        private async Task JoinedGuild(SocketGuild arg)
        {
            if (!Database.CreateNewGuild(arg.Id)) await Log(new LogMessage(LogSeverity.Critical, "Database", "Could not create new guild in Database"));
            else await Log(new LogMessage(LogSeverity.Debug, "Database", $"Created new Guild entry: {arg.Name}"));
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
            var message = messageParam as SocketUserMessage;
            if (message == null || message.Channel is IDMChannel) return;

            int argPos = 0;
            
            if (!(message.HasStringPrefix(Database.GetGuildPrefix((message.Channel as IGuildChannel).GuildId), ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

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
