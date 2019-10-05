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

            _client.SetGameAsync(Global.Status, "https://github.com/quinchs/Swiss001Bot", ActivityType.Streaming);

            _client.SetStatusAsync(UserStatus.Online);

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += LogMessage;

            _client.MessageReceived += HandleCommandAsync;

            _client.UserJoined += UpdateUserCount;

            _client.UserJoined += WelcomeMessage;

            _client.UserLeft += UpdateUserCount;

            _client.ReactionAdded += ReactionHandler; 

            _client.LatencyUpdated += _client_LatencyUpdated;

            _client.Ready += Init;

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Services loaded");
        }

        private async Task ReactionHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                await CheckVerification(arg1, arg2, arg3);
                await checkSub(arg1, arg2, arg3);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private async Task CheckVerification(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg2.Id == Global.VerificationChanID)
            {
                var emote = new Emoji("✅");
                if(arg3.Emote.Name == emote.Name)
                {
                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(arg3.UserId);
                    var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
                    if (user.Roles.Contains(unVertRole))
                    {
                        var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);
                        await user.AddRoleAsync(userRole);
                        Console.WriteLine($"Verified user {user.Username}#{user.Discriminator}");
                        EmbedBuilder eb2 = new EmbedBuilder()
                        {
                            Title = $"Verified {user.Mention}",
                            Color = Color.Green,
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = user.GetAvatarUrl(),
                                Text = $"{user.Username}#{user.Discriminator}"
                            },
                        };
                        var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
                        await chan.SendMessageAsync("", false, eb2.Build());
                        await user.RemoveRoleAsync(unVertRole);
                        string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
                        EmbedBuilder eb = new EmbedBuilder()
                        {
                            Title = $"***Welcome to Swiss001's Discord server!***",
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = user.GetAvatarUrl(),
                                Text = $"{user.Username}#{user.Discriminator}"
                            },
                            Description = welcomeMessage,
                            ThumbnailUrl = Global.WelcomeMessageURL,
                            Color = Color.Green
                        };
                        await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());
                        Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
                    }
                }
            }
        }

        private async Task checkSub(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg2.Id == Global.SubmissionChanID)
            {
                foreach (var item in Global.SubsList)
                {
                    if (!arg1.HasValue)
                    {
                        Global.ConsoleLog("Error, Reaction Message doesnt exist, Using ID to get message", ConsoleColor.Red);

                        var msg = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionChanID).GetMessageAsync(arg3.MessageId).Result;
                        if (item.botMSG.Embeds.First().Image.Value.Url == msg.Embeds.First().Image.Value.Url && item.botMSG.Embeds.First().Description == msg.Embeds.First().Description)
                        {
                            if (!arg3.User.Value.IsBot) //not a bot
                            {
                                string rs = "";
                                if (arg3.Emote.Name == item.checkmark.Name)
                                {
                                    //good img
                                    string curr = File.ReadAllText(Global.ButterFile);
                                    File.WriteAllText(Global.ButterFile, curr + "\n" + item.url);
                                    Global.ConsoleLog($"the image {item.url} has been approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}");
                                    await _client.GetUser(item.SubmitterID).SendMessageAsync($"Your butter submission was approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator} ({item.url})");
                                    await item.botMSG.DeleteAsync();
                                    await item.linkMsg.DeleteAsync();
                                    Global.SubsList.Remove(item);
                                    rs = "Accepted";
                                }
                                if (arg3.Emote.Name == item.Xmark.Name)
                                {
                                    //bad img
                                    Global.ConsoleLog($"the image {item.url} has been Denied by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}", ConsoleColor.Red);
                                    await item.botMSG.DeleteAsync();
                                    await item.linkMsg.DeleteAsync();
                                    Global.SubsList.Remove(item);
                                    var chan = _client.GetUser(item.SubmitterID);
                                    await chan.SendMessageAsync($"Your butter submission was Denied by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}, if you have any questions contact them ({item.url})");
                                    rs = "Denied";
                                }

                                EmbedBuilder eb = new EmbedBuilder()
                                {
                                    Title = "Submission Result",
                                    Color = Color.Blue,
                                    Description = $"The image {item.url} Submitted by {_client.GetUser(item.SubmitterID).Mention} has been **{rs}** by {arg3.User.Value.Mention} ({arg3.User.Value.Username}#{arg3.User.Value.Discriminator})",
                                    Footer = new EmbedFooterBuilder()
                                    {
                                        Text = "Result Autogen",
                                        IconUrl = _client.CurrentUser.GetAvatarUrl()
                                    }
                                };
                                await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionsLogChanID).SendMessageAsync("", false, eb.Build());
                            }
                        }
                    }
                    if (item.botMSG.Content == arg1.Value.Content)
                    {
                        if (!arg3.User.Value.IsBot) //not a bot
                        {
                            string rs = "";
                            if (arg3.Emote.Name == item.checkmark.Name)
                            {
                                //good img
                                string curr = File.ReadAllText(Global.ButterFile);
                                File.WriteAllText(Global.ButterFile, curr + "\n" + item.url);
                                Global.ConsoleLog($"the image {item.url} has been approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}");
                                await _client.GetUser(item.SubmitterID).SendMessageAsync($"Your butter submission was approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator} ({item.url})");
                                await item.botMSG.DeleteAsync();
                                await item.linkMsg.DeleteAsync();
                                Global.SubsList.Remove(item);
                                rs = "Accepted";
                            }
                            if (arg3.Emote.Name == item.Xmark.Name)
                            {
                                //bad img
                                Global.ConsoleLog($"the image {item.url} has been Denied by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}", ConsoleColor.Red);
                                await item.botMSG.DeleteAsync();
                                await item.linkMsg.DeleteAsync();
                                Global.SubsList.Remove(item);
                                var chan = _client.GetUser(item.SubmitterID);
                                await chan.SendMessageAsync($"Your butter submission was Denied by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}, if you have any questions contact them ({item.url})");
                                rs = "Denied";
                            }

                            EmbedBuilder eb = new EmbedBuilder()
                            {
                                Title = "Submission Result",
                                Color = Color.Blue,
                                Description = $"The image {item.url} has been **{rs}** by {arg3.User.Value.Mention} ({arg3.User.Value.Username}#{arg3.User.Value.Discriminator})",
                                Footer = new EmbedFooterBuilder()
                                {
                                    Text = "Result Autogen",
                                    IconUrl = _client.CurrentUser.GetAvatarUrl()
                                }
                            };
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionsLogChanID).SendMessageAsync("", false, eb.Build());
                        
                        }
                    }
                }
            }
        }

        private async Task _client_LatencyUpdated(int arg1, int arg2)
        {
            await UpdateUserCount(null);
        }

        private async Task WelcomeMessage(SocketGuildUser arg)
        {
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            await arg.AddRoleAsync(unVertRole);
            Console.WriteLine($"The member {arg.Username}#{arg.Discriminator} joined the guild");
        }
        internal static string WelcomeMessageBuilder(string orig, SocketGuildUser user)
        {
            if (orig.Contains("(USER)"))
                orig = orig.Replace("(USER)", $"<@{user.Id}>");

            if (orig.Contains("(USERCOUNT)"))
                orig = orig.Replace("(USERCOUNT)", Global.UserCount.ToString());
            return orig;
        }

        private async Task Init()
        {
            Global.ConsoleLog("Starting Init... \n\n Updating UserCounts...", ConsoleColor.DarkCyan);
            Global.UserCount = _client.GetGuild(Global.SwissGuildId).Users.Count;
            await UpdateUserCount(null);
            await UserSubCashing();
            Global.ConsoleLog("Finnished Init!", ConsoleColor.Black, ConsoleColor.DarkGreen);
        }
        private async Task UserSubCashing()
        {
            var messages = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionChanID).GetMessagesAsync().FlattenAsync();

            foreach (var message in messages)
            {
                if(message.Embeds.Count >= 1)
                {
                    if(message.Embeds.First().Description.Contains("This image was submitted by"))
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("<@(.*?)>");
                        System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex("LINK: (.*?)");
                        var link = r.Match(message.Embeds.First().Description);
                        var userid = r.Match(message.Embeds.First().Description).Value.Trim('<', '>', '@', '!');

                        Global.UnnaprovedSubs ua = new Global.UnnaprovedSubs()
                        {
                            linkMsg = message,
                            botMSG = messages.FirstOrDefault(x => x.Content.Contains(link.Value)),
                            checkmark = new Emoji("✓"),
                            Xmark = new Emoji("❌"),
                            SubmitterID = Convert.ToUInt64(userid),
                            url = message.Embeds.First().Image.Value.Url
                        };
                        Global.SubsList.Add(ua);
                    }
                }
            }
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
            Global.UserCount = usercount;
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
        public async Task EchoMessage(SocketCommandContext Context)
        {
            if(Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
            { 
                var echomsg = Context.Message.Content.Replace($"{Global.Preflix}echo", "");
                var chan = Context.Guild.GetTextChannel(592463507124125706).SendMessageAsync(echomsg);
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
                if (msg.Content.StartsWith($"{Global.Preflix}echo")) { await EchoMessage(context); return; }
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
