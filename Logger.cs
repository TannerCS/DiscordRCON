using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRCON
{
    class Logger
    {
        public static async Task LogAsync(LogMessage msg)
        {
            await PrintColor(msg);
        }

        private static void WriteToLog(LogMessage msg)
        {
            //Formatting: [00:00:00 00:00:00] [SEVERITY] Exception message goes here
            File.AppendAllText("log.txt", $"[{DateTime.Now:dd:MM:yy HH:mm:ss}] [{msg.Severity.ToString().ToUpper()}] {msg.Message}\n");
        }

        public static void Log(LogMessage msg)
        {
            PrintColor(msg);
        }

        static async Task PrintColor(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical: //Only logging Critical and Error messages. Everything else will clutter the logs
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    WriteToLog(msg);
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    break;
                case LogSeverity.Error: //Only logging Critical and Error messages. Everything else will clutter the logs
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor();
                    WriteToLog(msg);
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
    }
}
