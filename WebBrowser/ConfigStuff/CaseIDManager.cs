using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.WebBrowserJavaScriptInjections.deserialization;

namespace WebBrowser.Config
{
    class CaseIDManager
    {
        public static CaseIDManager _Instance;

        private bool alreadySetCasesToOpen = false;

        //in config file be able to decide if you want individual risk factor for each case, need dictionary stuff to convert text to the cases

        //Dictionary within dictionary, ex. dict["25"]["10"] (25% lvl 10)

        Dictionary<string, Dictionary<string, string>> allBoxIds = new Dictionary<string, Dictionary<string, string>>
        {
            ["2"] = new Dictionary<string, string> //Level 2
            {
                ["5"] = "Qm94OjUxMTQ1NQ",//"Qm94OjI1MDMz", //5%
                ["10"] = "Qm94OjUxMTQ2Ng", //"Qm94OjI1MDQ0", //10%
                ["20"] = "Qm94OjUxMTQ3Nw", //"Qm94OjI1MDU1", //20%
                ["25"] = "Qm94OjUxMTQ4OA", // "Qm94OjI1MDY2", //25%
                ["40"] = "Qm94OjUxMTQ5OQ", //"Qm94OjI1MDc3", //40%
                ["50"] = "Qm94OjUxMTUxMA",//"Qm94OjI1MDg4", //50%
                ["60"] = "Qm94OjUxMTUyMQ", //"Qm94OjI1MDk5", //60%
            },
            ["10"] = new Dictionary<string, string> //Level 10
            {
                ["5"] = "Qm94OjUxMTQ1Ng",//"Qm94OjI1MDM0", //5%
                ["10"] = "Qm94OjUxMTQ2Nw",//"Qm94OjI1MDQ1", //10%
                ["20"] = "Qm94OjUxMTQ3OA", //"Qm94OjI1MDU2", //20%
                ["25"] = "Qm94OjUxMTQ4OQ",//"Qm94OjI1MDY3", //25%
                ["40"] = "Qm94OjUxMTUwMA",//"Qm94OjI1MDc4", //40%
                ["50"] = "Qm94OjUxMTUxMQ",//"Qm94OjI1MDg5", //50%
                ["60"] = "Qm94OjUxMTUyMg",//"Qm94OjI1MTAw", //60%
            },
            ["20"] = new Dictionary<string, string> //Level 20
            {
                ["5"] = "Qm94OjUxMTQ1Nw",//"Qm94OjI1MDM1", //5%
                ["10"] = "Qm94OjUxMTQ2OA", //10%
                ["20"] = "Qm94OjUxMTQ3OQ", //20%
                ["25"] = "Qm94OjUxMTQ5MA", //25%
                ["40"] = "Qm94OjUxMTUwMQ", //40%
                ["50"] = "Qm94OjUxMTUxMg", //50%
                ["60"] = "Qm94OjUxMTUyMw", //60%
            },
            ["30"] = new Dictionary<string, string> //Level 30
            {
                ["5"] = "Qm94OjUxMTQ1OA",//"Qm94OjI1MDM2", //5%
                ["10"] = "Qm94OjUxMTQ2OQ", //10%
                ["20"] = "Qm94OjUxMTQ4MA", //20%
                ["25"] = "Qm94OjUxMTQ5MQ", //25%
                ["40"] = "Qm94OjUxMTUwMg", //40%
                ["50"] = "Qm94OjUxMTUxMw", //50%
                ["60"] = "Qm94OjUxMTUyNA", //60%
            },
            ["40"] = new Dictionary<string, string> //Level 40
            {
                ["5"] = "Qm94OjUxMTQ1OQ",//"Qm94OjI1MDM3", //5%
                ["10"] = "Qm94OjUxMTQ3MA", //10%
                ["20"] = "Qm94OjUxMTQ4MQ", //20%
                ["25"] = "Qm94OjUxMTQ5Mg", //25%
                ["40"] = "Qm94OjUxMTUwMw", //40%
                ["50"] = "Qm94OjUxMTUxNA", //50%
                ["60"] = "Qm94OjUxMTUyNQ", //60%
            },
            ["50"] = new Dictionary<string, string> //Level 50
            {
                ["5"] = "Qm94OjUxMTQ2MA",//"Qm94OjI1MDM4", //5%
                ["10"] = "Qm94OjUxMTQ3MQ", //10%
                ["20"] = "Qm94OjUxMTQ4Mg", //20%
                ["25"] = "Qm94OjUxMTQ5Mw", //25%
                ["40"] = "Qm94OjUxMTUwNA", //40%
                ["50"] = "Qm94OjUxMTUxNQ", //50%
                ["60"] = "Qm94OjUxMTUyNg", //60%
            },
            ["60"] = new Dictionary<string, string> //Level 60
            {
                ["5"] = "Qm94OjUxMTQ2MQ",//"Qm94OjI1MDM5", //5%
                ["10"] = "Qm94OjUxMTQ3Mg", //10%
                ["20"] = "Qm94OjUxMTQ4Mw", //20%
                ["25"] = "Qm94OjUxMTQ5NA", //25%
                ["40"] = "Qm94OjUxMTUwNQ", //40%
                ["50"] = "Qm94OjUxMTUxNg", //50%
                ["60"] = "Qm94OjUxMTUyNw", //60%
            },
            ["70"] = new Dictionary<string, string> //Level 70
            {
                ["5"] = "Qm94OjUxMTQ2Mg",//"Qm94OjI1MDQw", //5%
                ["10"] = "Qm94OjUxMTQ3Mw", //10%
                ["20"] = "Qm94OjUxMTQ4NA", //20%
                ["25"] = "Qm94OjUxMTQ5NQ", //25%
                ["40"] = "Qm94OjUxMTUwNg", //40%
                ["50"] = "Qm94OjUxMTUxNw", //50%
                ["60"] = "Qm94OjUxMTUyOA", //60%
            },
            ["80"] = new Dictionary<string, string> //Level 80
            {
                ["5"] = "Qm94OjUxMTQ2Mw",//"Qm94OjI1MDQx", //5%
                ["10"] = "Qm94OjUxMTQ3NA", //10%
                ["20"] = "Qm94OjUxMTQ4NQ", //20%
                ["25"] = "Qm94OjUxMTQ5Ng", //25%
                ["40"] = "Qm94OjUxMTUwNw", //40%
                ["50"] = "Qm94OjUxMTUxOA", //50%
                ["60"] = "Qm94OjUxMTUyOQ", //60%
            },
            ["90"] = new Dictionary<string, string> //Level 90
            {
                ["5"] = "Qm94OjUxMTQ2NA",//"Qm94OjI1MDQy", //5%
                ["10"] = "Qm94OjUxMTQ3NQ", //10%
                ["20"] = "Qm94OjUxMTQ4Ng", //20%
                ["25"] = "Qm94OjUxMTQ5Nw", //25%
                ["40"] = "Qm94OjUxMTUwOA", //40%
                ["50"] = "Qm94OjUxMTUxOQ", //50%
                ["60"] = "Qm94OjUxMTUzMA", //60%
            },
            ["100"] = new Dictionary<string, string> //Level 100
            {
                ["5"] = "Qm94OjUxMTQ2NQ",//"Qm94OjI1MDQz", //5%
                ["10"] = "Qm94OjUxMTQ3Ng", //10%
                ["20"] = "Qm94OjUxMTQ4Nw", //20%
                ["25"] = "Qm94OjUxMTQ5OA", //25%
                ["40"] = "Qm94OjUxMTUwOQ", //40%
                ["50"] = "Qm94OjUxMTUyMA", //50%
                ["60"] = "Qm94OjUxMTUzMQ", //60%
            },
        };

        public List<string> caseIdsToOpen = new List<string>();
        public List<string> caseIdsToPvpBattle = new List<string>();

        public List<CaseOpened> openedCasesResults = new List<CaseOpened>();

        private int playerlvl = -1;

        public CaseIDManager()
        {
            _Instance = this;
        }

        public void LoadCasesToOpen()
        {
            foreach (CaseConfig cc in ConfigManager._Instance.GetConfigFile().casesToOpen)
            {
                string boxid = allBoxIds[cc.level][cc.risk];

                if (playerlvl != -1)
                {
                    if (Convert.ToInt32(cc.level) > playerlvl)
                    {
                        Form1._instance.printToConsole($"Case level {cc.level} is too high level for your account!");
                        continue;
                    }
                }

                caseIdsToOpen.Add(boxid);
            }

            foreach (CaseConfig cc in ConfigManager._Instance.GetConfigFile().casesToPvpbattle)
            {
                string boxid = allBoxIds[cc.level][cc.risk];

                if (playerlvl != -1)
                {
                    if (Convert.ToInt32(cc.level) > playerlvl)
                    {
                        Form1._instance.printToConsole($"Case level {cc.level} is too high level for your account!");
                        continue;
                    }
                }

                caseIdsToPvpBattle.Add(boxid);
            }
        }

        public (string levelKey, string percentKey)? GetBoxKeysFromValue(string targetValue)
        {
            foreach (var outerPair in allBoxIds)
            {
                foreach (var innerPair in outerPair.Value)
                {
                    if (innerPair.Value == targetValue)
                    {
                        return (outerPair.Key, innerPair.Key); // Return both keys
                    }
                }
            }

            return null; // Not found
        }

        public void GotPlayerLevel(int level)
        {
            //determine what cases to do
            //remove any cases that are too high leveled even if the user has it in their list
            if (playerlvl != -1)
            {
                return; //already set the level
            }
            playerlvl = level;

            LoadCasesToOpen();
        }

        public CaseOpened GetMostValuableItemUnboxed()
        {
            double highestValue = 0;
            CaseOpened bco = new CaseOpened();

            if (openedCasesResults == null) return null;

            foreach (CaseOpened co in openedCasesResults)
            {
                if (co == null) continue;
                if (co.data == null) continue;
                if (co.data.openBox == null) continue;
                if (co.data.openBox.boxOpenings == null || co.data.openBox.boxOpenings.Count == 0) continue;
                var firstOpening = co.data.openBox.boxOpenings[0];
                if (firstOpening == null) continue;
                if (firstOpening.userItem == null) continue;

                double value = firstOpening.userItem.consumeValue;
                if (value > highestValue)
                {
                    highestValue = value;
                    bco = co;
                }
            }

            return bco;
        }

        public CaseOpened GetBestItemRolled()
        {
            double highestValue = 0;
            CaseOpened bco = new CaseOpened();
            foreach (CaseOpened co in openedCasesResults)
            {
                if (co.data.openBox.boxOpenings[0].roll.value > highestValue)
                {
                    highestValue = co.data.openBox.boxOpenings[0].roll.value;
                    bco = co;
                }
            }

            return bco;
        }

        public double GetPlayerMainWalletBalance(User userdata)
        {
            foreach (UserWallet wallet in userdata.wallets)
            {
                if (wallet.name == "MAIN")
                {
                    return wallet.amount;
                }
            }

            return 0.0;
        }

        public (string level, string percent) GetLevelPercent(string boxId)
        {
            foreach (var levelEntry in allBoxIds)
            {
                foreach (var percentEntry in levelEntry.Value)
                {
                    if (percentEntry.Value == boxId)
                    {
                        return (levelEntry.Key, percentEntry.Key); // or break out of both loops
                    }
                }
            }

            return ("", "");
        }
    }
}
