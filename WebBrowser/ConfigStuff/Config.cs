using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.Config
{
    class Config
    {
        public string taskSchedulerTaskName { get; set; } = "CSGORoll Daily Collector";
        public bool autoUpdateProgram { get; set; } = true;

        public string keepRetryingAfterFailInfo { get; set; } = "If true the program will stay open till all the cases are opened, otherwise it will try again in 1hr";
        public bool keepRetryingAfterFail { get; set; } = false;

        public string proxyAddress { get; set; } = "";
        public string proxyTypeInfo { get; set; } = "Proxy type options: 'http' or 'https'";
        public string proxyType { get; set; } = "";
        public string proxyUsername { get; set; } = "";
        public string proxyPassword { get; set; } = "";

        public string infoString { get; set; } = "You can remove/add/reorder the list below based on how/which cases you want to open. PvP battles will run first, incase that fails I recommend keeping them here too";
        public List<CaseConfig> casesToOpen { get; set; } = new List<CaseConfig>
        {
            new CaseConfig()
            {
                level = "2",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "10",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "20",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "30",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "40",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "50",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "60",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "70",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "80",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "90",
                risk = "50",
            },
            new CaseConfig()
            {
                level = "100",
                risk = "50",
            },
        };
        public string infoStringpvp { get; set; } = "This list behaves the same as above but allows you to battle it. Providing invalid settings will cause it to try and correct it!";
        public List<CaseConfig> casesToPvpbattle { get; set; } = new List<CaseConfig>
        {

        };

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


        public TimeSpan triggerTime { get; set; }
        public string disableAffiliate { get; set; } = "why would you want to disbale this?";
        public bool enableDiscordWebhook { get; set; } = false;
        public string discordWebhookURL { get; set; } = "be careful people can brute force discord webhooks and spam messages";
        public string identifier { get; set; } = "for special hosting feature that can be found in the discord server";
        public bool enableLocalBotComm { get; set; } = false;
        public int commport { get; set; } = 0;
        public string configVersion { get; set; } = "2.0.0";
    }

    class CaseConfig
    {
        public string level { get; set; }
        public string risk { get; set; }
    }
}
