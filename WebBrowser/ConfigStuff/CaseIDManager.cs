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
                ["5"] = "Qm94OjI1MDMz", //5%
                ["10"] = "Qm94OjI1MDQ0", //10%
                ["20"] = "Qm94OjI1MDU1", //20%
                ["25"] = "Qm94OjI1MDY2", //25%
                ["40"] = "Qm94OjI1MDc3", //40%
                ["50"] = "Qm94OjI1MDg4", //50%
                ["60"] = "Qm94OjI1MDk5", //60%
            },
            ["10"] = new Dictionary<string, string> //Level 10
            {
                ["5"] = "Qm94OjI1MDM0", //5%
                ["10"] = "Qm94OjI1MDQ1", //10%
                ["20"] = "Qm94OjI1MDU2", //20%
                ["25"] = "Qm94OjI1MDY3", //25%
                ["40"] = "Qm94OjI1MDc4", //40%
                ["50"] = "Qm94OjI1MDg5", //50%
                ["60"] = "Qm94OjI1MTAw", //60%
            },
            ["20"] = new Dictionary<string, string> //Level 20
            {
                ["5"] = "Qm94OjI1MDM1", //5%
                ["10"] = "Qm94OjI1MDQ2", //10%
                ["20"] = "Qm94OjI1MDU3", //20%
                ["25"] = "Qm94OjI1MDY4", //25%
                ["40"] = "Qm94OjI1MDc5", //40%
                ["50"] = "Qm94OjI1MDkw", //50%
                ["60"] = "Qm94OjI1MTAx", //60%
            },
            ["30"] = new Dictionary<string, string> //Level 30
            {
                ["5"] = "Qm94OjI1MDM2", //5%
                ["10"] = "Qm94OjI1MDQ3", //10%
                ["20"] = "Qm94OjI1MDU4", //20%
                ["25"] = "Qm94OjI1MDY5", //25%
                ["40"] = "Qm94OjI1MDgw", //40%
                ["50"] = "Qm94OjI1MDkx", //50%
                ["60"] = "Qm94OjI1MTAy", //60%
            },
            ["40"] = new Dictionary<string, string> //Level 40
            {
                ["5"] = "Qm94OjI1MDM3", //5%
                ["10"] = "Qm94OjI1MDQ4", //10%
                ["20"] = "Qm94OjI1MDU5", //20%
                ["25"] = "Qm94OjI1MDcw", //25%
                ["40"] = "Qm94OjI1MDgx", //40%
                ["50"] = "Qm94OjI1MDky", //50%
                ["60"] = "Qm94OjI1MTAz", //60%
            },
            ["50"] = new Dictionary<string, string> //Level 50
            {
                ["5"] = "Qm94OjI1MDM4", //5%
                ["10"] = "Qm94OjI1MDQ5", //10%
                ["20"] = "Qm94OjI1MDYw", //20%
                ["25"] = "Qm94OjI1MDcx", //25%
                ["40"] = "Qm94OjI1MDgy", //40%
                ["50"] = "Qm94OjI1MDkz", //50%
                ["60"] = "Qm94OjI1MTA0", //60%
            },
            ["60"] = new Dictionary<string, string> //Level 60
            {
                ["5"] = "Qm94OjI1MDM5", //5%
                ["10"] = "Qm94OjI1MDUw", //10%
                ["20"] = "Qm94OjI1MDYx", //20%
                ["25"] = "Qm94OjI1MDcy", //25%
                ["40"] = "Qm94OjI1MDgz", //40%
                ["50"] = "Qm94OjI1MDk0", //50%
                ["60"] = "Qm94OjI1MTA1", //60%
            },
            ["70"] = new Dictionary<string, string> //Level 70
            {
                ["5"] = "Qm94OjI1MDQw", //5%
                ["10"] = "Qm94OjI1MDUx", //10%
                ["20"] = "Qm94OjI1MDYy", //20%
                ["25"] = "Qm94OjI1MDcz", //25%
                ["40"] = "Qm94OjI1MDg0", //40%
                ["50"] = "Qm94OjI1MDk1", //50%
                ["60"] = "Qm94OjI1MTA2", //60%
            },
            ["80"] = new Dictionary<string, string> //Level 80
            {
                ["5"] = "Qm94OjI1MDQx", //5%
                ["10"] = "Qm94OjI1MDUy", //10%
                ["20"] = "Qm94OjI1MDYz", //20%
                ["25"] = "Qm94OjI1MDc0", //25%
                ["40"] = "Qm94OjI1MDg1", //40%
                ["50"] = "Qm94OjI1MDk2", //50%
                ["60"] = "Qm94OjI1MTA3", //60%
            },
            ["90"] = new Dictionary<string, string> //Level 90
            {
                ["5"] = "Qm94OjI1MDQy", //5%
                ["10"] = "Qm94OjI1MDUz", //10%
                ["20"] = "Qm94OjI1MDY0", //20%
                ["25"] = "Qm94OjI1MDc1", //25%
                ["40"] = "Qm94OjI1MDg2", //40%
                ["50"] = "Qm94OjI1MDk3", //50%
                ["60"] = "Qm94OjI1MTA4", //60%
            },
            ["100"] = new Dictionary<string, string> //Level 100
            {
                ["5"] = "Qm94OjI1MDQz", //5%
                ["10"] = "Qm94OjI1MDU0", //10%
                ["20"] = "Qm94OjI1MDY1", //20%
                ["25"] = "Qm94OjI1MDc2", //25%
                ["40"] = "Qm94OjI1MDg3", //40%
                ["50"] = "Qm94OjI1MDk4", //50%
                ["60"] = "Qm94OjI1MTA5", //60%
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
            foreach (CaseOpened co in openedCasesResults)
            {
                if (co.data.openBox.boxOpenings[0].userItem.consumeValue > highestValue)
                {
                    highestValue = co.data.openBox.boxOpenings[0].userItem.consumeValue;
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
