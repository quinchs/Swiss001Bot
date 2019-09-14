﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SwissBot.Global;

namespace SwissBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("butter")]
        public async Task butter(string url)
        {
            //add butter link to butter file
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if(result)
            {
                string curr = File.ReadAllText(Global.ButterFile);
                File.WriteAllText(ButterFile, curr + url + "\n");
                ConsoleLog($"User {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} has submitted the image {url}");
                await Context.Channel.SendMessageAsync("Added to the butter database, to view some butter do `\"butter`. Abuse of this command will not be tolerated");
            }
            else { await Context.Channel.SendMessageAsync("That is not a valad URL!"); }
        }
        [Command("butter")]
        public async Task butter()
        {
            //add butter link to butter file
            if(Context.Message.Attachments.Count >= 1)
            {
                foreach(var attachment in Context.Message.Attachments)
                {
                    var url = attachment.Url;
                    var filename = attachment.Filename;
                    await Context.Channel.SendMessageAsync($"Added `{filename}` ({url}) to the butter database!");
                    string curr = File.ReadAllText(Global.ButterFile);
                    File.WriteAllText(ButterFile, curr + url + "\n");
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
                    ImageUrl = Global.WelcomeMessageURL,
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
                        data.Preflix = iName.ToCharArray()[0];
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
