﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord.Rest;
using Discord;
using System.Net.Http;
using System.Net;

namespace SwissBot
{
    class Global
    {
        public static char Preflix { get; set; }
        private static string ConfigPath = $"{Environment.CurrentDirectory}\\Data\\Config.json";
        private static string cMSGPath = $"{Environment.CurrentDirectory}\\Data\\CashedMSG.MSG";
        private static string ConfigSettingsPath = $"{Environment.CurrentDirectory}\\Data\\ConfigPerms.json";
        public static string aiResponsePath = $"{Environment.CurrentDirectory}\\Data\\Responses.AI";
        public static string LinksDirpath = $"{Environment.CurrentDirectory}\\LinkLogs";
        public static string Status { get; set; }
        public static string Token { get; set; }
        public static DiscordSocketClient Client { get; set; }
        public static ulong SwissGuildId { get; set; }
        internal static JsonItems CurrentJsonData;
        public static ulong DeveloperRoleId { get; set; }
        public static int UserCount { get; set; }
        public static ulong SwissBotDevGuildID { get; set; }
        public static string MessageLogsDir = $"{Environment.CurrentDirectory}\\Messagelogs";
        public static string CommandLogsDir = $"{Environment.CurrentDirectory}\\Commandlogs";
        public static string ButterFile = $"{Environment.CurrentDirectory}\\Data\\Landings.butter";
        public static string ButterFilesDirPath = $"{Environment.CurrentDirectory}\\Data\\ButterFiles";
        public static ulong LogsChannelID { get; set; }
        public static ulong DebugChanID { get; set; }
        public static List<UnnaprovedSubs> SubsList = new List<UnnaprovedSubs>();
        public static ulong TestingCat { get; set; }
        public static Dictionary<string, bool> ConfigSettings { get; set; }
        public static ulong StatsChanID { get; set; }
        public static ulong StatsTotChanID { get; set; }
        public static ulong WelcomeMessageChanID { get; set; }
        public static string WelcomeMessage { get; set; }
        public static ulong SubmissionChanID { get; set; }
        public static string WelcomeMessageURL { get; set; }
        public static ulong ModeratorRoleID { get; set; }
        public static ulong MemberRoleID { get; set; }
        public static ulong UnverifiedRoleID { get; set; }
        public static ulong VerificationChanID { get; set; }
        public static List<GiveAway> GiveAwayGuilds { get; set; }
        public static ulong VerificationLogChanID { get; set; }
        public static ulong SubmissionsLogChanID { get; set; }
        public static ulong MilestonechanID { get; set; }
        public static int AutoSlowmodeTrigger { get; set; }
        public static bool AutoSlowmodeToggle { get; set; }
        public static ulong giveawayCreatorChanId { get; set; }
        public static ulong giveawayChanID { get; set; }
        public static ulong BotAiChanID { get; set; }
        public static Dictionary<string, List<LogItem>> linkLogs { get; set; }
        public static Dictionary<string, List<LogItem>> messageLogs { get; set; }
        public static Dictionary<string, List<LogItem>> commandLogs { get; set; }


        public static string ApiKey { get; set; }
        internal static Dictionary<string, string> jsonItemsList { get; private set; }
        internal static Dictionary<string, string> JsonItemsListDevOps { get; private set; }
        public struct GiveAway
        {
            public int Seconds { get; set; }
            public string GiveAwayItem { get; set; }
            public ulong GiveAwayUser { get; set; }
            public string discordInvite { get; set; }
            public int numWinners { get; set; }
            public RestUserMessage giveawaymsg { get; set; }
            public GiveawayGuildObj giveawayguild { get; set; }
        }
        public class LogItem
        {
            public string username { get; set; }
            public string id { get; set; }
            public string date { get; set; }
            public string channel { get; set; }
            public string message { get; set; }
        }
        public class GiveawayGuildObj
        {
            public ulong guildID { get; set; }
            public bool bansActive { get; set; }
            public List<GiveawayUser> giveawayEntryMembers { get; set; }
            public RestGuild guildOBJ { get; set; }
            public GiveawayGuildObj create(RestGuild guild)
            {
                this.guildID = guild.Id;
                this.bansActive = false;
                giveawayEntryMembers = new List<GiveawayUser>();
                guildOBJ = guild;
                return this;
            }
            public void startBans() { this.bansActive = true; }

            public void removeUser(GiveawayUser bannedUser, GiveawayUser remainingUser)
            {
                if(giveawayEntryMembers.Contains(bannedUser))
                {
                    if(giveawayEntryMembers.Contains(remainingUser))
                    {
                        remainingUser.bans++;
                        remainingUser.bannedUsers.Add(bannedUser);
                        giveawayEntryMembers.Remove(bannedUser);
                    }
                }
            }
        }
        public class GiveawayUser
        {
            public int bans { get; set; }
            public string DiscordName { get; set; }
            public SocketGuildUser user { get; set; }
            public ulong id { get; set; }
            public List<GiveawayUser> bannedUsers { get; set; }
            
        }
        public static void ReadConfig()
        {
            if (!Directory.Exists(MessageLogsDir)) { Directory.CreateDirectory(MessageLogsDir); }
            if (!Directory.Exists(CommandLogsDir)) { Directory.CreateDirectory(CommandLogsDir); }
            if (!File.Exists(aiResponsePath)) { File.Create(aiResponsePath); }

            var data = JsonConvert.DeserializeObject<JsonItems>(File.ReadAllText(ConfigPath));
            jsonItemsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            JsonItemsListDevOps = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            ConfigSettings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(ConfigSettingsPath));
            foreach(var item in ConfigSettings)
                if (item.Value == false)
                    jsonItemsList.Remove(item.Key);

            JsonItemsListDevOps.Remove("Token");
            CurrentJsonData = data;
            Preflix = data.Preflix;
            WelcomeMessageChanID = data.WelcomeMessageChanID;
            WelcomeMessage = data.WelcomeMessage;
            WelcomeMessageURL = data.WelcomeMessageURL;
            Status = data.Status;
            giveawayChanID = data.giveawayChanID;
            giveawayCreatorChanId = data.giveawayCreatorChanId;
            Token = data.Token;
            StatsChanID = data.StatsChanID;
            SwissGuildId = data.SwissGuildID;
            DeveloperRoleId = data.DeveloperRoleId;
            SwissBotDevGuildID = data.SwissTestingGuildID;
            LogsChannelID = data.LogsChannelID;
            DebugChanID = data.DebugChanID;
            SubmissionChanID = data.SubmissionChanID;
            TestingCat = data.TestingCatigoryID;
            ModeratorRoleID = data.ModeratorRoleID;
            MemberRoleID = data.MemberRoleID;
            AutoSlowmodeTrigger = data.AutoSlowmodeTrigger;
            ApiKey = data.ApiKey;
            AutoSlowmodeToggle = data.AutoSlowmodeToggle;
            UnverifiedRoleID = data.UnverifiedRoleID;
            VerificationChanID = data.VerificationChanID;
            VerificationLogChanID = data.VerificationLogChanID;
            SubmissionsLogChanID = data.SubmissionsLogChanID;
            MilestonechanID = data.MilestonechanID;
            BotAiChanID = data.BotAiChanID;
            StatsTotChanID = data.StatsTotChanID;
        }
        public static void SaveConfigPerms(Dictionary<string, bool> nConfigPerm)
        {
            string json = JsonConvert.SerializeObject(nConfigPerm, Formatting.Indented);
            File.WriteAllText(ConfigSettingsPath, json);
            ConsoleLog("Saved New configPerm items. here is the new JSON \n " + json + "\n Saving...", ConsoleColor.Black, ConsoleColor.DarkYellow);
            ReadConfig();
        }
        public static List<string> getUnvertCash()
        {
            return File.ReadAllLines(cMSGPath).ToList();
        }
        public static void saveUnvertCash(List<string> newDat)
        {
            string nw = "";
            foreach (var id in newDat)
                nw += $"{id}\n";
            File.WriteAllText(cMSGPath, nw);
        }
        public static void SaveConfig(JsonItems newData)
        {
            string jsonS = JsonConvert.SerializeObject(newData, Formatting.Indented);
            newData.Token = "N#########################";
            string conJson = JsonConvert.SerializeObject(newData, Formatting.Indented);
            File.WriteAllText(ConfigPath, jsonS);
            ConsoleLog("Saved New config items. here is the new JSON \n " + conJson + "\n Saving...", ConsoleColor.DarkYellow);
            ReadConfig();
        }
        public static void SaveConfig(Dictionary<string, bool> newPerms)
        {
            string jsonS = JsonConvert.SerializeObject(newPerms);
            ConsoleLog("Saved New configPerms items. here is the new JSON \n " + jsonS + "\n Saving...", ConsoleColor.Blue);
            File.WriteAllText(ConfigSettingsPath, jsonS);
        }
        public class JsonItems
        {
            public string Token { get; set; }
            public string Status { get; set; }
            public char Preflix { get; set; }
            public ulong SwissGuildID { get; set; }
            public ulong SwissTestingGuildID { get; set; }
            public ulong TestingCatigoryID { get; set; }
            public ulong DeveloperRoleId { get; set; }
            public ulong LogsChannelID { get; set; }
            public ulong DebugChanID { get; set; }
            public ulong StatsChanID { get; set; }
            public ulong SubmissionChanID { get; set; }
            public ulong WelcomeMessageChanID { get; set; }
            public string WelcomeMessage { get; set; }
            public string WelcomeMessageURL { get; set; }
            public ulong VerificationLogChanID { get; set; }
            public ulong ModeratorRoleID { get; set; }
            public ulong MemberRoleID { get; set; }
            public ulong UnverifiedRoleID { get; set; }
            public ulong VerificationChanID { get; set; }
            public ulong SubmissionsLogChanID { get; set; }
            public ulong MilestonechanID { get; set; }
            public bool AutoSlowmodeToggle { get; set; }
            public ulong BotAiChanID { get; set; }
            public ulong StatsTotChanID { get; set; }
            public ulong giveawayChanID { get; set; }
            public ulong giveawayCreatorChanId { get; set; }
            public string ApiKey { get; set; }
            public int AutoSlowmodeTrigger { get; set; }
        }
        public static void ConsoleLog(string ConsoleMessage, ConsoleColor FColor = ConsoleColor.Green, ConsoleColor BColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = FColor;
            Console.BackgroundColor = BColor;
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + ConsoleMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public static async void SendExeption(Exception ex)
        {
            EmbedBuilder b = new EmbedBuilder();
            b.Color = Color.Red;
            b.Description = $"The following info is for an Exeption, `TARGET`\n\n```{ex.TargetSite}```\n`EXEPTION`\n\n```{ex.Message}```\n`SOURCE`\n\n```{ex.Source}```\n";
            b.Footer = new EmbedFooterBuilder();
            b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
            b.Title = "Bot Command Error!";
            await Client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
            await Client.GetGuild(Global.SwissBotDevGuildID).GetTextChannel(622164033902084145).SendMessageAsync("", false, b.Build());
        }
        public struct UnnaprovedSubs
        {
            public IMessage linkMsg { get; set; }
            public IMessage botMSG { get; set; }
            public string url { get; set; }
            public Emoji checkmark { get; set; }
            public Emoji Xmark { get; set; }
            public ulong SubmitterID { get; set; }
        }
        public struct ApiData
        {
            public string apiKey { get; set; }
            public Modules.Commands.JsonGuildObj JsonGuildObj { get; set; }
        }
        public static async Task<string> SendJsontoNeoney()
        {
            const string url = "https://api.neoney.xyz/swiss/addBackup";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            ApiData data = new ApiData()
            {
                apiKey = ApiKey,
                JsonGuildObj = await Modules.Commands.GetGuildObj()
            };
            
            
            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));

            // this is important - make sure you specify type this way
            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
            {
                result = rdr.ReadToEnd();
            }
            return result;
        }
        public static async Task<string> getNeoneyStuff()
        {
            const string url = "https://api.neoney.xyz/swiss/listbackups";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "GET";
            request.Headers.Add("authorization", "Bearer AS89d8sjscnjZ)09=0-_+9aks309JjncaA014389");
            // turn our request string into a byte stream
            
            // this is important - make sure you specify type this way
           
            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
            {
                result = rdr.ReadToEnd();
            }
            return result;
        }
    }
}
