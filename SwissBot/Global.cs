using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace SwissBot
{
    class Global
    {
        public static char Preflix { get; set; }
        private static string ConfigPath = $"{Environment.CurrentDirectory}\\Data\\Config.json";
        public static string Status { get; set; }
        public static string Token { get; set; }
        public static DiscordSocketClient Client { get; set; }
        public static ulong SwissGuildId { get; set; }
        internal static JsonItems CurrentJsonData;
        public static ulong DeveloperRoleId { get; set; }
        public static ulong SwissBotDevGuildID { get; set; }
        public static string MessageLogsDir = $"{Environment.CurrentDirectory}\\Messagelogs";
        public static string CommandLogsDir = $"{Environment.CurrentDirectory}\\Commandlogs";
        public static ulong LogsChannelID { get; set; }
        public static ulong DebugChanID { get; set; }
        public static ulong TestingCat { get; set; }
        public static ulong StatsChanID { get; set; }
        internal static Dictionary<string, string> jsonItemsList { get; private set; }
        internal static Dictionary<string, string> JsonItemsListDevOps { get; private set; }

        public static void ReadConfig()
        {
            if (!Directory.Exists(MessageLogsDir)) { Directory.CreateDirectory(MessageLogsDir); }
            if (!Directory.Exists(CommandLogsDir)) { Directory.CreateDirectory(CommandLogsDir); }

            var data = JsonConvert.DeserializeObject<JsonItems>(File.ReadAllText(ConfigPath));
            jsonItemsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            JsonItemsListDevOps = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            jsonItemsList.Remove("Token");
            jsonItemsList.Remove("Status");
            jsonItemsList.Remove("Preflix");
            jsonItemsList.Remove("SwissGuildID");
            jsonItemsList.Remove("SwissTestingGuildID");
            jsonItemsList.Remove("TestingCatigory");
            jsonItemsList.Remove("LogsChannelID");
            jsonItemsList.Remove("DebugChanID");
            jsonItemsList.Remove("StatsChanID");
            JsonItemsListDevOps.Remove("Token");
            CurrentJsonData = data;
            Preflix = data.Preflix;
            Status = data.Status;
            Token = data.Token;
            StatsChanID = data.StatsChanID;
            SwissGuildId = data.SwissGuildID;
            DeveloperRoleId = data.DeveloperRoleId;
            SwissBotDevGuildID = data.SwissTestingGuildID;
            LogsChannelID = data.LogsChannelID;
            DebugChanID = data.DebugChanID;
            TestingCat = data.TestingCatigory;
        }
        public static void SaveConfig(JsonItems newData)
        {
            string jsonS = JsonConvert.SerializeObject(newData);
            ConsoleLog("Saved New config items. here is the new JSON \n " + jsonS + "\n Saving...", ConsoleColor.DarkYellow);
            File.WriteAllText(ConfigPath, jsonS);
        }
        public struct JsonItems
        {
            public string Token { get; set; }
            public string Status { get; set; }
            public char Preflix { get; set; }
            public ulong SwissGuildID { get; set; }
            public ulong SwissTestingGuildID { get; set; }
            public ulong TestingCatigory { get; set; }
            public ulong DeveloperRoleId { get; set; }
            public ulong LogsChannelID { get; set; }
            public ulong DebugChanID { get; set; }
            public ulong StatsChanID { get; set; }
        }
        public static void ConsoleLog(string ConsoleMessage, ConsoleColor FColor = ConsoleColor.Green, ConsoleColor BColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = FColor;
            Console.BackgroundColor = BColor;
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + ConsoleMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
