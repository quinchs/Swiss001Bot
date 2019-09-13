using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SwissBot
{
    class CommandHandler
    {
        private static DiscordSocketClient _client;
        private CommandService _service;
        
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.SetGameAsync(Global.Status, null, ActivityType.Playing);

            _client.SetStatusAsync(UserStatus.Online);

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += LogMessage;

            _client.MessageReceived += HandleCommandAsync;

            _client.UserJoined += UpdateUserCount;

            _client.UserLeft += UpdateUserCount;

            _client.Ready += Init;

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Services loaded");
        }

        private async Task Init()
        {
            Global.ConsoleLog("Starting Init... \n\n Updating UserCounts...", ConsoleColor.DarkCyan);
            await UpdateUserCount(null);
            Global.ConsoleLog("Finnished Init!", ConsoleColor.Black, ConsoleColor.DarkGreen);
        }

        private async Task UpdateUserCount(SocketGuildUser arg)
        {
            var users = _client.GetGuild(Global.SwissGuildId).Users;
            int usercount = 0;
            foreach (var user in users)
            {
                if (!user.IsBot)
                    usercount++;
            }
            Console.WriteLine($"Ucount {usercount}, usersSCount{users.Count}");
            await _client.GetGuild(Global.SwissGuildId).GetVoiceChannel(Global.StatsChanID).ModifyAsync(x => 
            {
                x.Name = $"Human Users: {usercount}";
            });
        }

        private async Task LogMessage(SocketMessage arg)
        {
            //Log messages to txt file
            string logMsg = "";
            logMsg += $"[{DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
            logMsg += $"USER: {arg.Author.Username}#{arg.Author.Discriminator} CHANNEL: {arg.Channel.Name} MESSAGE: {arg.Content}";
            var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
            if (File.Exists(Global.MessageLogsDir + $"\\{name}.txt"))
            {
                string curr = File.ReadAllText(Global.MessageLogsDir + $"\\{name}.txt");
                File.WriteAllText(Global.MessageLogsDir + $"\\{name}.txt", $"{curr}\n{logMsg}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Logged message (from {arg.Author.Username})");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else
            {
                File.Create(Global.MessageLogsDir + $"\\{name}.txt").Close();
                File.WriteAllText(Global.MessageLogsDir + $"\\{name}.txt", $"{logMsg}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Logged message (from {arg.Author.Username}) and created new logfile");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
        }
        public async Task HandleCommandAsync(SocketMessage s)
        {
           
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);


            int argPos = 0;
            if (msg.HasCharPrefix(Global.Preflix, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    EmbedBuilder b = new EmbedBuilder();
                    b.Color = Color.Red;
                    b.Description = $"The following info is the Command error info, `{msg.Author.Username}#{msg.Author.Discriminator}` tried to use the `{msg}` Command in {msg.Channel}: \n \n **COMMAND ERROR**: ```{result.Error.Value}``` \n \n **COMMAND ERROR REASON**: ```{result.ErrorReason}```";
                    b.Author = new EmbedAuthorBuilder();
                    b.Author.Name = msg.Author.Username + "#" + msg.Author.Discriminator;
                    b.Author.IconUrl = msg.Author.GetAvatarUrl();
                    b.Footer = new EmbedFooterBuilder();
                    b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                    b.Title = "Bot Command Error!";
                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
                    await _client.GetGuild(Global.SwissBotDevGuildID).GetTextChannel(622164033902084145).SendMessageAsync("", false, b.Build());
                }
                await HandleCommandresult(result, msg);
            }
        }
        internal async Task HandleCommandresult(IResult result, SocketUserMessage msg)
        {
            string logMsg = "";
            logMsg += $"[UTC TIME - {DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
            string completed = resultformat(result.IsSuccess);
            if (!result.IsSuccess)
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed} ERROR TYPE: {result.Error.Value} EXCEPTION: {result.ErrorReason}";
            else
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed}";
            var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
            if (File.Exists(Global.CommandLogsDir + $"\\{name}.txt"))
            {
                string curr = File.ReadAllText(Global.CommandLogsDir + $"\\{name}.txt");
                File.WriteAllText(Global.CommandLogsDir + $"\\{name}.txt", $"{curr}\n{logMsg}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Logged Command (from {msg.Author.Username})");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else
            {
                File.Create(Global.MessageLogsDir + $"\\{name}.txt").Close();
                File.WriteAllText(Global.CommandLogsDir + $"\\{name}.txt", $"{logMsg}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Logged Command (from {msg.Author.Username}) and created new logfile");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            if (result.IsSuccess)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = Color.Green;
                eb.Title = "**Command Log**";
                eb.Description = $"The Command {msg.Content.Split(' ').First()} was used in {msg.Channel.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}";
                eb.Footer = new EmbedFooterBuilder();
                eb.Footer.Text = "Command Autogen";
                eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, eb.Build());
            }

        }
        internal static string resultformat(bool isSuccess)
        {
            if (isSuccess)
                return "Sucess";
            if (!isSuccess)
                return "Failed";
            return "Unknown";
        }
    }
}
