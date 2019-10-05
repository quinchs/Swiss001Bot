using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SwissBot.Global;

namespace SwissBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task help()
        {
            if (Context.Guild.Id == SwissBotDevGuildID || Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
            {
                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = "***SwissBot Help***",
                    Color = Color.Green,
                    Description = "These are the following commands for SwissBot!\n\n" +
                $"```{Global.Preflix}modify```\n**Parameters** - `{Global.Preflix}modify (ITEMNAME) (NEWVALUE)`\n use `{Global.Preflix}modify list` to view the `.config` file\n\n" +
                $"```{Global.Preflix}welcome```\n Use this command to test the welcome message\n\n" +
                $"```{Global.Preflix}butter```\n**Parameters** - `{Global.Preflix}butter (LINK)`\n use this command to get or submit butter landings!\n\n" +
                $"```{Global.Preflix}commandlogs```\n**Parameters** - `{Global.Preflix}commandlogs (LOG_NAME)`\n use `{Global.Preflix}commandlogs list` to view all command logs\n\n" +
                $"```{Global.Preflix}messagelogs```\n**Parameters** - `{Global.Preflix}messagelogs (LOG_NAME)`\n use `{Global.Preflix}messagelogs list` to view all message logs\n\n" +
                $"```{Global.Preflix}help```\n View this help message :D\n" +
                $"```{Global.Preflix}muteusers```\n use this command in a voice channel to server mute all members in a voice channel\n **Does not mute staff**\n" +
                $"```{Global.Preflix}unmuteusers```\n use this command in a voice channel to unserver mute all members in a voice channel\n **Does not mute/unmute staff**",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                        Text = "Help Autogen"
                    },
                };
                await Context.Channel.SendMessageAsync("", false, eb.Build());
            }
            else
            {
                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = "***SwissBot Help***",
                    Color = Color.Green,
                    Description = "These are the following commands for SwissBot!\n\n" +
               // $"**{Global.Preflix}**modify**\n**Parameters** - ```{Global.Preflix}modify (ITEMNAME) (NEWVALUE)```\n use `{Global.Preflix}modify list` to view the `.config` file\n\n" +
                //$"**{Global.Preflix}welcome**\n Use this command to test the welcome message\n\n" +
                $"`{Global.Preflix}butter`\n**Parameters** - ```{Global.Preflix}butter (LINK)```\n use this command to get or submit butter landings!\n\n" +
                //$"**{Global.Preflix}commandlogs**\n**Parameters** - ```{Global.Preflix}commandlogs (LOG_NAME)```\n use `{Global.Preflix}commandlogs list` to view all command logs\n\n" +
                //$"**{Global.Preflix}messagelogs**\n**Parameters** - ```{Global.Preflix}messagelogs (LOG_NAME)```\n use `{Global.Preflix}messagelogs list` to view all message logs\n\n" +
                $"`{Global.Preflix}help`\n View this help message :D",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                        Text = "Help Autogen"
                    },
                };
                await Context.Channel.SendMessageAsync("", false, eb.Build());
            }
        }
        [Command("butter")]
        public async Task butter(string url)
        {
            //add butter link to butter file
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if(result)
            {
                if(Context.Channel.Id == Global.SubmissionChanID)
                {
                    string curr = File.ReadAllText(Global.ButterFile);
                    File.WriteAllText(ButterFile, curr + url + "\n");
                    ConsoleLog($"User {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} has submitted the image {url}");
                    var msg = await Context.Channel.SendMessageAsync($"Added {url} to the butter database!");
                    await Context.Message.DeleteAsync();
                    await Task.Delay(5000);
                    await msg.DeleteAsync();
                }
                else
                {
                    UnnaprovedSubs us = new UnnaprovedSubs();
                   
                    us.url = url;
                    us.SubmitterID = Context.Message.Author.Id;
                    await Context.Channel.SendMessageAsync($"Thank you, {Context.Message.Author.Mention} for the submission, we will get back to you!");
                    EmbedBuilder eb = new EmbedBuilder();
                    //eb.ImageUrl = us.url;
                    eb.Title = "**Butter Submission**";
                    eb.Description = $"This image was submitted by {Context.Guild.GetUser(us.SubmitterID).Mention}.";
                    eb.Color = Color.Orange;
                    var msg = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync("", false, eb.Build());
                    var msg2 = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync(us.url);

                    await msg2.AddReactionAsync(new Emoji("✅"));
                    await msg2.AddReactionAsync(new Emoji("❌"));
                    us.checkmark = new Emoji("✅");
                    us.Xmark = new Emoji("❌");
                    us.botMSG = msg2;
                    us.linkMsg = msg;
                    SubsList.Add(us);
                    var curr = getUnvertCash();
                    curr.Add(msg.Id.ToString());
                    saveUnvertCash(curr);
                }
            }
            else { await Context.Channel.SendMessageAsync("That is not a valad URL!"); }
        }
        [Command("purge")]
        public async Task purge(uint amount)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(Context.Guild.Roles.FirstOrDefault(x=>x.Id == 622156934778454016)))
            {
                var messages = await this.Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                foreach (var message in messages)
                {
                    await message.DeleteAsync();
                }
                const int delay = 5000;
                var m = await this.ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have permission to use this command!");
            }
        }
        [Command("terminate")]
        public async Task term()
        {
            if(Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                await Context.Channel.SendMessageAsync("Shutting down!");
                Environment.Exit(1);
            }
            else
            {
                if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId)) || Context.User.Id == Context.Client.CurrentUser.Id)
                {
                    await Context.Channel.SendMessageAsync("Shutting down Overlord!");
                    Environment.Exit(1);
                }
            }
        }
        [Command("muteusers")]
        public async Task muteusers()
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null)
            {
                if(Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync($"Starting to mute members...");
                    int u = 0;
                    foreach(var user in Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel.Users)
                    {
                        var r2 = Context.Guild.GetUser(user.Id).Roles;
                        var adminrolepos2 = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
                        var rolepos2 = r2.FirstOrDefault(x => x.Position >= adminrolepos);
                        if (rolepos2 == null)
                        {
                            await user.ModifyAsync(x => x.Mute = true);
                            u++;
                        }
                    }
                    await Context.Channel.SendMessageAsync($"Muted {u} members");
                }
            }
        }
        [Command("unmuteusers")]
        public async Task unmuteusers()
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null)
            {
                if (Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync($"Starting to Unmute members...");
                    int u = 0;

                    foreach (var user in Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel.Users)
                    {
                        if (user.IsMuted)
                        {
                            await user.ModifyAsync(x => x.Mute = false);
                            u++;
                        }
                    }
                    await Context.Channel.SendMessageAsync($"UnMuted {u} members");
                }
            }
        }
        [Command("butter")]
        public async Task butter()
        {
            //add butter link to butter file
            if(Context.Message.Attachments.Count >= 1)
            {
                foreach(var attachment in Context.Message.Attachments)
                {
                    UnnaprovedSubs us = new UnnaprovedSubs();

                    us.url = attachment.Url;
                    us.SubmitterID = Context.Message.Author.Id;
                    await Context.Channel.SendMessageAsync($"Thank you, {Context.Message.Author.Mention} for the submission, we will get back to you!");
                    EmbedBuilder eb = new EmbedBuilder();
                    //eb.ImageUrl = us.url;
                    eb.Title = "**Butter Submission**";
                    eb.Description = $"This image was submitted by {Context.Guild.GetUser(us.SubmitterID).Mention}. LINK: {us.url}";
                    eb.Color = Color.Orange;
                    var msg = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync("", false, eb.Build());
                    var msg2 = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync(us.url);

                    await msg2.AddReactionAsync(new Emoji("✅"));
                    await msg2.AddReactionAsync(new Emoji("❌"));
                    us.checkmark = new Emoji("✅");
                    us.Xmark = new Emoji("❌");
                    us.botMSG = msg2;
                    us.linkMsg = msg;
                    SubsList.Add(us);
                    var curr = getUnvertCash();
                    curr.Add(msg.Id.ToString());
                    saveUnvertCash(curr);
                }
            }
            else //get a random butter
            {
                Random r = new Random();
                int max = File.ReadAllLines(ButterFile).Count();
                int num = r.Next(0, max);
                string link = File.ReadAllLines(ButterFile)[num];
                await Context.Channel.SendMessageAsync($"50, 40, 30, 20, 10, **Butter** \n {link}");
            }
        }
        [Command("configperms")]
        public async Task configperm(string name, string newValue)
        {
            if(Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if(Global.ConfigSettings.Keys.Contains(name))
                {
                    bool perm = true ? newValue == "true" : false;
                    Global.ConfigSettings.Remove(name);
                    ConfigSettings.Add(name, perm);
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Title = "**Updated Config**";
                    eb.Footer = new EmbedFooterBuilder();
                    eb.Footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                    eb.Footer.Text = "Command Autogen";
                    eb.Color = Color.Green;
                    eb.Description = "Updated the Config Permissions, Here is the new Config Permissions";
                    string items = "";
                    foreach (var item in ConfigSettings) 
                        items += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n";
                    eb.Description += $"\n{items}";
                    Global.SaveConfigPerms(ConfigSettings);
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
            }
        }
        [Command("configperms")]
        public async Task configperm(string name)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string list = "";
                    foreach (var item in ConfigSettings)
                        list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n";
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Title = "**Config Permission List**",
                        Description = $"**here is the config list**\n {list}",
                        Color = Color.Green,
                        Footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                            Text = "Command Autogen"
                        },
                    };
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
                else
                    await Context.Channel.SendMessageAsync($"Not a valad argument, please do `{Global.Preflix}configperms list` do view the config items, to change one type `{Global.Preflix}configperms (ITEM NAME) (VALUE)`");
            }
        }
        [Command("welcome")]
        public async Task welcome()
        {
            if(Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
            {
                var arg = Context.Guild.GetUser(Context.Message.Author.Id);
                string welcomeMessage = CommandHandler.WelcomeMessageBuilder(Global.WelcomeMessage, arg);

                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = $"***Welcome to Swiss001's Discord server!***",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = arg.GetAvatarUrl(),
                        Text = $"{arg.Username}#{arg.Discriminator}"
                    },
                    Description = welcomeMessage,
                    ThumbnailUrl = Global.WelcomeMessageURL,
                    Color = Color.Green
                };
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                Global.ConsoleLog($"WelcomeMessage for {arg.Username}#{arg.Discriminator}", ConsoleColor.Blue);
            }
        }
        [Command("commandlogs")]
        public async Task logs(string name)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string names = "";
                    foreach (var file in Directory.GetFiles(Global.CommandLogsDir))
                    {
                        names += file.Split('\\').Last() + "\n";
                    }
                    if (names == "")
                        names = "There are currently no Log files :\\";
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Color.Green;
                    eb.Title = "**Command logs List**";
                    eb.Description = $"Here are the current Command Logs, To fetch one do `\"commandlogs (name)` \n ```{names}```";
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
                else
                {
                    if (File.Exists(Global.CommandLogsDir + $"\\{name}"))
                    {
                        await Context.Channel.SendFileAsync(Global.CommandLogsDir + $"\\{name}", $"Here is the Log **{name}**");
                    }
                    else
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Red;
                        eb.Title = "**Command logs List**";
                        eb.Description = $"The file {name} does not exist, try doing `\"commandlogs list` to view all the command logs";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                }
            }
            else
            {
                if (Context.Channel.Id == Global.LogsChannelID)
                {
                    if (name == "list")
                    {
                        string names = "";
                        foreach (var file in Directory.GetFiles(Global.CommandLogsDir))
                        {
                            names += file.Split('\\').Last() + "\n";
                        }
                        if (names == "")
                            names = "There are currently no Log files :\\";
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Green;
                        eb.Title = "**Command logs List**";
                        eb.Description = $"Here are the current Command Logs, To fetch one do `\"commandlogs (name)` \n ```{names}```";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                    else
                    {
                        if (File.Exists(Global.CommandLogsDir + $"\\{name}"))
                        {
                            await Context.Channel.SendFileAsync(Global.CommandLogsDir + $"\\{name}", $"Here is the Log **{name}**");
                        }
                        else
                        {
                            EmbedBuilder eb = new EmbedBuilder();
                            eb.Color = Color.Red;
                            eb.Title = "**Command logs List**";
                            eb.Description = $"The file {name} does not exist, try doing `\"commandlogs list` to view all the command logs";
                        }
                    }
                }
            }
        }
        [Command("messagelogs")]
        public async Task mlogs(string name)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string names = "";
                    foreach (var file in Directory.GetFiles(Global.MessageLogsDir))
                    {
                        names += file.Split('\\').Last() + "\n";
                    }
                    if (names == "")
                        names = "There are currently no Log files :\\";
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Color.Green;
                    eb.Title = "**Message logs List**";
                    eb.Description = $"Here are the current Message Logs, To fetch one do `\"messagelogs (name)` \n ```{names}```";

                }
                else
                {
                    if (File.Exists(Global.MessageLogsDir + $"\\{name}"))
                    {
                        await Context.Channel.SendFileAsync(Global.MessageLogsDir + $"\\{name}", $"Here is the Log **{name}**");
                    }
                    else
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Red;
                        eb.Title = "**Message logs List**";
                        eb.Description = $"The file {name} does not exist, try doing `\"messagelogs list` to view all the command logs";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                }
            }
            else
            {
                if (Context.Channel.Id == Global.LogsChannelID)
                {
                    if (name == "list")
                    {
                        string names = "";
                        foreach (var file in Directory.GetFiles(Global.MessageLogsDir))
                        {
                            names += file.Split('\\').Last() + "\n";
                        }
                        if (names == "")
                            names = "There are currently no Log files :\\";
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Green;
                        eb.Title = "**Message logs List**";
                        eb.Description = $"Here are the current Message Logs, To fetch one do `\"messagelogs (name)` \n ```{names}```";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                    else
                    {
                        if (File.Exists(Global.MessageLogsDir + $"\\{name}"))
                        {
                            await Context.Channel.SendFileAsync(Global.MessageLogsDir + $"\\{name}", $"Here is the Log **{name}**");
                        }
                        else
                        {
                            EmbedBuilder eb = new EmbedBuilder();
                            eb.Color = Color.Red;
                            eb.Title = "**Message logs List**";
                            eb.Description = $"The file {name} does not exist, try doing `\"messagelogs list` to view all the command logs";
                        }
                    }
                }
            }
        }
        [Command("modify")]
        public async Task modify(string configItem, string value)
        {
            string newvalue = value.Replace("\\", " ");
            if (Context.Guild.Id == Global.SwissBotDevGuildID)//allow full modify
            {
                if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                {
                    JsonItems data = Global.CurrentJsonData;
                    data = modifyJsonData(data, configItem, newvalue);
                    if (data.Token != null)
                    {
                        Global.SaveConfig(data);
                        await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                        EmbedBuilder b = new EmbedBuilder();
                        b.Footer = new EmbedFooterBuilder();
                        b.Footer.Text = "**Dev Config**";
                        b.Title = "Dev Config List";
                        string list = "**Here is the current config file** \n";
                        foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                        b.Description = list;
                        b.Color = Color.Green;
                        b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                    }

                }
                else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
            }
            if (Context.Guild.Id == Global.SwissGuildId)
            {
                if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))//allow some modify
                {
                    if (Global.jsonItemsList.Keys.Contains(configItem))
                    {
                        JsonItems data = Global.CurrentJsonData;
                        data = modifyJsonData(data, configItem, newvalue);
                        if (data.Token != null)
                        {
                            Global.SaveConfig(data);
                            await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                            EmbedBuilder b = new EmbedBuilder();
                            b.Footer = new EmbedFooterBuilder();
                            b.Footer.Text = "**Admin Config**";
                            b.Title = "Admin Config List";
                            string list = "**Here is the current config file** \n";
                            foreach (var item in Global.jsonItemsList) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                            b.Description = list;
                            b.Color = Color.Green;
                            b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                            await Context.Channel.SendMessageAsync("", false, b.Build());
                        }
                    }
                    else
                    {
                        if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                        {
                            EmbedBuilder b = new EmbedBuilder();
                            b.Color = Color.Red;
                            b.Title = "You need Better ***PERMISSION***";
                            b.Description = "You do not have permission to modify this item, if you think this is incorrect you can DM quin#3017 for help";

                            await Context.Channel.SendMessageAsync("", false, b.Build());
                        }
                        else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
                    }

                }
            }
        }
        [Command("modify")]
        public async Task modify(string configItem)
        {
            if (configItem == "list")
            {
                if (Context.Guild.Id == Global.SwissBotDevGuildID)
                {
                    EmbedBuilder b = new EmbedBuilder();
                    b.Footer = new EmbedFooterBuilder();
                    b.Footer.Text = "**Dev Config**";
                    b.Title = "Dev Config List";
                    string list = "**Here is the current config file** \n";
                    foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                    b.Description = list;
                    b.Color = Color.Green;
                    b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                    await Context.Channel.SendMessageAsync("", false, b.Build());
                }
                else
                {
                    
                    if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
                    {
                        EmbedBuilder b = new EmbedBuilder();
                        b.Footer = new EmbedFooterBuilder();
                        b.Footer.Text = "**Admin Config**";
                        b.Title = "Admin Config List";
                        string list = "**Here is the current config file, not all items are here, if you wish to view more items please contact Thomas or Swiss, because they control the config items you can modify!** \n";
                        string itemsL = "";
                        foreach (var item in Global.jsonItemsList) { itemsL += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                        if(itemsL == "") { list = "**Sorry but there is nothing here or you do not have permission to change anything yet :/**"; }
                        b.Description = list + itemsL;
                        b.Color = Color.Green;
                        b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync($"No value was provided for the variable `{configItem}`");
            }
        }
        internal JsonItems modifyJsonData(JsonItems data, string iName, string iValue)
        {
            try
            {
                switch (iName)
                {
                    case "Status":
                        data.Status = iValue;
                        Context.Client.SetGameAsync(iValue, null, ActivityType.Listening);
                        break;
                    case "Preflix":
                        data.Preflix = iValue.ToCharArray()[0];
                        break;
                    case "SwissGuildID":
                        data.SwissGuildID = Convert.ToUInt64(iValue);
                        break;
                    case "SwissTestingGuildID":
                        data.SwissTestingGuildID = Convert.ToUInt64(iValue);
                        break;
                    case "TestingCatigory":
                        data.TestingCatigoryID = Convert.ToUInt64(iValue);
                        break;
                    case "DeveloperRoleId":
                        data.DeveloperRoleId = Convert.ToUInt64(iValue);
                        break;
                    case "LogsChannelID":
                        data.LogsChannelID = Convert.ToUInt64(iValue);
                        break;
                    case "DebugChanID":
                        data.DebugChanID = Convert.ToUInt64(iValue);
                        break;
                    case "SubmissionChanID":
                        data.SubmissionChanID = Convert.ToUInt64(iValue);
                        break;
                    case "WelcomeMessageChanID":
                        data.WelcomeMessageChanID = Convert.ToUInt64(iValue);
                        break;
                    case "ModeratorRoleID":
                        data.ModeratorRoleID = Convert.ToUInt64(iValue);
                        break;
                    case "WelcomeMessage":
                        data.WelcomeMessage = (iValue);
                        break;
                    case "WelcomeMessageURL":
                        data.WelcomeMessageURL = (iValue);
                        break;
                    case "StatsChanID":
                        data.StatsChanID = Convert.ToUInt64(iValue);
                        break;
                }
                return data;
            }
            catch (Exception ex)
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = "Exeption!",
                    Description = $"**{ex}**"
                };
                Context.Channel.SendMessageAsync("", false, b.Build());
                return data;
            }
        }
    }
}
