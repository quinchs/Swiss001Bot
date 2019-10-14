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

            _client.SetStatusAsync(UserStatus.DoNotDisturb);

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
                Global.ConsoleLog($"Reaction handler error: {ex.Message}", ConsoleColor.Red);
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
                        if (item.botMSG.Embeds.First().Description == msg.Embeds.First().Description)
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
            await checkMembers();
        }
        internal static async Task checkMembers()
        {
            switch (_client.GetGuild(Global.SwissGuildId).MemberCount)
            {
                case 5000:
                    await SendMilestone(5000);
                    break;
                case 6000:
                    await SendMilestone(6000);
                    break;
                case 7000:
                    await SendMilestone(7000);
                    break;
                case 7500:
                    await SendMilestone(7500);
                    break;
            }
        }
        public static async Task SendMilestone(int count, ulong chanid = 0)
        {
            SocketTextChannel MilestoneChan;
            if (chanid != 0) { MilestoneChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(chanid); }
            else { MilestoneChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.MilestonechanID); }
            var memberList = _client.GetGuild(Global.SwissGuildId).Users.ToList();
            Random r = new Random();
            var mem1 = memberList[r.Next(0, memberList.Count)];
            var mem2 = memberList[r.Next(0, memberList.Count)];
            var mem3 = memberList[r.Next(0, memberList.Count)];
            var msg = await MilestoneChan.SendMessageAsync("@everyone", false, new EmbedBuilder() {
                Color = Color.Blue,
                Title = $":tada: We did it! Congratulations on {count} Members!! :tada:",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Swiss001's Discord",
                    IconUrl = _client.GetUser(365958535768702988).GetAvatarUrl(),
                    
                },
                ThumbnailUrl = Global.WelcomeMessageURL,
                Description = $"Thank you @everyone we made it to {count} Members, Congrats everyone :tada: :tada:!\n\nI would like to thank {mem1.Mention} for there support on this discord and also {mem2.Mention} for sticking up for the community, and a **THICC** thanks to {mem3.Mention} for making us all laugh a little more, and a HUGE thanks to the {count} people who make this community what it is today\n\nFrom,\n*Swiss001 Staff Team.*",
                Url = "https://www.youtube.com/channel/UCYiaHzwtsww6phfxwUtZv8w"
            }.Build());
            Global.ConsoleLog("\n\n Milestone Reached! \n\n", ConsoleColor.Blue);
            await msg.ModifyAsync(x => x.Content = " ");
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
            Global.ConsoleLog("Finnished UserCount", ConsoleColor.Cyan);
            await UserSubCashing();
            await AddUnVert();
            await CheckVerts();
            Global.ConsoleLog("Finnished Init!", ConsoleColor.Black, ConsoleColor.DarkGreen);
        }
        private async Task CheckVerts()
        {
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);

            foreach (var user in _client.GetGuild(Global.SwissGuildId).Users.Where(x => x.Roles.Contains(unVertRole) && x.Roles.Count == 2))
            {
                await user.AddRoleAsync(userRole);
                Global.ConsoleLog($"Found the user {user.Username}#{user.Discriminator} who hasnt recieved verification yet. Gave them Member role", ConsoleColor.White, ConsoleColor.DarkBlue);
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

            }
            //IUserMessage sMessage = (IUserMessage)await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationChanID).GetMessageAsync(627680940155469844);
            //var emote = new Emoji("✅");
            //var reActs = await sMessage.GetReactionUsersAsync(emote, 5000).FlattenAsync();
            //foreach (var rUsers in reActs.ToList())
            //{
            //    var user = _client.GetGuild(Global.SwissGuildId).GetUser(rUsers.Id);
            //    if(user != null)
            //    {
            //        if (user.Roles.Contains(unVertRole))
            //        {
            //            await user.AddRoleAsync(userRole);
            //            Global.ConsoleLog($"Found the user {user.Username}#{user.Discriminator} who hasnt recieved verification yet. Gave them Member role", ConsoleColor.White, ConsoleColor.DarkBlue);
            //            EmbedBuilder eb2 = new EmbedBuilder()
            //            {
            //                Title = $"Verified {user.Mention}",
            //                Color = Color.Green,
            //                Footer = new EmbedFooterBuilder()
            //                {
            //                    IconUrl = user.GetAvatarUrl(),
            //                    Text = $"{user.Username}#{user.Discriminator}"
            //                },
            //            };
            //            var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
            //            await chan.SendMessageAsync("", false, eb2.Build());
            //            await user.RemoveRoleAsync(unVertRole);
            //            string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
            //            EmbedBuilder eb = new EmbedBuilder()
            //            {
            //                Title = $"***Welcome to Swiss001's Discord server!***",
            //                Footer = new EmbedFooterBuilder()
            //                {
            //                    IconUrl = user.GetAvatarUrl(),
            //                    Text = $"{user.Username}#{user.Discriminator}"
            //                },
            //                Description = welcomeMessage,
            //                ThumbnailUrl = Global.WelcomeMessageURL,
            //                Color = Color.Green
            //            };
            //            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());

            //        }
            //    }
            //    else { }
            //}
        }
        private async Task AddUnVert()
        {
            var noRoleUsers = _client.GetGuild(Global.SwissGuildId).Users.Where(x => x.Roles.Count == 1).ToList();
            Global.ConsoleLog($"Found {noRoleUsers.Count} Users without verification, Adding the Unverivied role...", ConsoleColor.Cyan);
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            int i = 0;
            foreach (var user in noRoleUsers) { await user.AddRoleAsync(unVertRole); i++; Global.ConsoleLog($"Gave Unvert role to {user.Username}, {noRoleUsers.Count - i} users left", ConsoleColor.White, ConsoleColor.DarkBlue); }
        }
        private async Task UserSubCashing()
        {
            var messages = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionChanID).GetMessagesAsync().FlattenAsync();

            foreach (var message in messages)
            {
                if(message.Embeds.Count >= 1 && message.Embeds.First().Description != null)
                {
                    if(message.Embeds.First().Description.Contains("This image was submitted by"))
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("<@(.*?)>");
                        System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex("LINK: (.*?);");
                        string disc = message.Embeds.First().Description;
                        var link = r2.Match(disc).Groups[1].Value;
                        var userid = r.Match(disc).Value.Trim('<', '>', '@', '!');

                        try
                        {
                            Global.UnnaprovedSubs ua = new Global.UnnaprovedSubs()
                            {
                                linkMsg = message,
                                botMSG = messages.FirstOrDefault(x => x.Content.Contains(link)),
                                checkmark = new Emoji("✅"),
                                Xmark = new Emoji("❌"),
                                SubmitterID = Convert.ToUInt64(userid),
                                url = link
                            };
                            Global.SubsList.Add(ua);
                        }
                        catch(Exception ex) { }
                       
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
