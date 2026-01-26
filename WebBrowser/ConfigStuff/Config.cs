using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.Config
{
    class Config
    {
        public string infoStringRisk { get; set; } = "Choose what risk you want to unbox. Options: 60, 50, 40, 25, 20, 10, 5";
        public int risk { get; set; } = 60;
        public string infoStringCaseValue { get; set; } = "Choose what value case you would like to unbox. Options: 1 - Low, 2 - Medium, 3 - High";
        public int caseValue { get; set; } = 3;

        public string infoStringPvpBattleCase { get; set; } = "Set to true to pvp battle your daily free case";
        public bool pvpBattleCase { get; set; } = false;

        public string infoStringPvpStrategyInfo1 { get; set; } = "Strategy Options:";
        public string infoStringPvpStrategyInfo2 { get; set; } = "'HIGHEST_SUM' (Regular)  'LOWEST_SUM' (Crazy)";
        public string infoStringPvpStrategyInfo3 { get; set; } = "'HIGHEST_BET_PAYOUT' (Clutch Regular)  'LOWEST_BET_PAYOUT' (Clutch Crazy)";
        public string infoStringPvpStrategyInfo4 { get; set; } = "'HIGHEST_LAST_BET_PAYOUT' (Terminal Regular)  'LOWEST_LAST_BET_PAYOUT' (Terminal Crazy)";
        public string infoStringPvpStrategyInfo5 { get; set; } = "'RANDOM_TICKET' (Jackpot Regular)  'INVERSE_RANDOM_TICKET' (Jackpot Crazy)";
        public string pvpStrategy { get; set; } = "HIGHEST_SUM";

        public string infoStringPvpNumOfPlayers { get; set; } = "Options: 2 to 6";
        public int pvpNumOfPlayers { get; set; } = 4;

        public string infoStringPvpNumOfTeams1 { get; set; } = "Options: 1 to 4";
        public string infoStringPvpNumOfTeams2 { get; set; } = "if 1 number of players can be 2 to 6 - 1 is used for shared mode";
        public string infoStringPvpNumOfTeams3 { get; set; } = "if 2 number of players can be either 2, 4, 6";
        public string infoStringPvpNumOfTeams4 { get; set; } = "if 3 number of players can be either 3, 6";
        public string infoStringPvpNumOfTeams5 { get; set; } = "if 4 number of players can only be 4";
        public int pvpNumOfTeams { get; set; } = 2;

        public string infoStringPvpNumOfPlayersOnTeam { get; set; } = "Number of players per team";
        public int pvpNumOfPlayersOnTeam { get; set; } = 2;

        public string infoStringPvpMode { get; set; } = "Options: 'TEAM' or 'SINGLE'";
        public string pvpMode { get; set; } = "TEAM";



        public string taskSchedulerTaskName { get; set; } = "CSGORoll Daily Collector";
        public bool autoUpdateProgram { get; set; } = true;

        public string proxyAddress { get; set; } = "";
        public string proxyTypeInfo { get; set; } = "Proxy type options: 'http' or 'https'";
        public string proxyType { get; set; } = "";
        public string proxyUsername { get; set; } = "";
        public string proxyPassword { get; set; } = "";

        public string keepRetryingAfterFailInfo { get; set; } = "If true the program will stay open till all the cases are opened, otherwise it will try again in 1hr";
        public bool keepRetryingAfterFail { get; set; } = false;
        public string infoStringTimeoutLength = "How long in minutes before the program times out and retries in an hour";
        public int timeoutLength = 15;

        public bool clearCookiesOnLaunch { get; set; } = false;


        public TimeSpan triggerTime { get; set; }
        public string disableAffiliate { get; set; } = "why would you want to disbale this?";
        public bool enableDiscordWebhook { get; set; } = false;
        public string discordWebhookURL { get; set; } = "be careful people can brute force discord webhooks and spam messages";
        public string identifier { get; set; } = "used to identify instance when running multiple instances, intended for hosting in our discord server";
        public bool enableLocalBotComm { get; set; } = false;
        public int commport { get; set; } = 0;
        public string configVersion { get; set; } = "2.3.0";
    }
}
