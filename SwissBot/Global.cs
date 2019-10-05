using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord.Rest;
using Discord;

namespace SwissBot
{
    class Global
    {
        public static char Preflix { get; set; }
        private static string ConfigPath = $"{Environment.CurrentDirectory}\\Data\\Config.json";
        private static string cMSGPath = $"{Environment.CurrentDirectory}\\Data\\CashedMSG.MSG";
        private static string ConfigSettingsPath = $"{Environment.CurrentDirectory}\\Data\\ConfigPerms.json";
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
        public static ulong WelcomeMessageChanID { get; set; }
        public static string WelcomeMessage { get; set; }
        public static ulong SubmissionChanID { get; set; }
        public static string WelcomeMessageURL { get; set; }
        public static ulong ModeratorRoleID { get; set; }
        public static ulong MemberRoleID { get; set; }
        public static ulong UnverifiedRoleID { get; set; }
        public static ulong VerificationChanID { get; set; }
        public static ulong VerificationLogChanID { get; set; }
        public static ulong SubmissionsLogChanID { get; set; }

        internal static Dictionary<string, string> jsonItemsList { get; private set; }
        internal static Dictionary<string, string> JsonItemsListDevOps { get; private set; }

        public static void ReadConfig()
        {
            if (!Directory.Exists(MessageLogsDir)) { Directory.CreateDirectory(MessageLogsDir); }
            if (!Directory.Exists(CommandLogsDir)) { Directory.CreateDirectory(CommandLogsDir); }

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
            UnverifiedRoleID = data.UnverifiedRoleID;
            VerificationChanID = data.VerificationChanID;
            VerificationLogChanID = data.VerificationLogChanID;
            SubmissionsLogChanID = data.SubmissionsLogChanID;
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
        public struct JsonItems
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
        }
        public static void ConsoleLog(string ConsoleMessage, ConsoleColor FColor = ConsoleColor.Green, ConsoleColor BColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = FColor;
            Console.BackgroundColor = BColor;
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + ConsoleMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
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
    }
}
