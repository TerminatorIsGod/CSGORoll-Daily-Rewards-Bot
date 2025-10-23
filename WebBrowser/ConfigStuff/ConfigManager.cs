using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebBrowser.Config
{
    class ConfigManager
    {
        public static ConfigManager _Instance;

        public string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //public string configFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "configs"));

        private string LogsFile = "";
        private string ConfigFile = "";

        private Config config = null;

        TimeSpan userResetTimeSpan;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public ConfigManager()
        {
            _Instance = this;
            LogsFile = Path.Combine(exeFolder, "logs.Egario");
            ConfigFile = Path.Combine(exeFolder, "config.json");
        }

        public string GetLogsFileLocation()
        {
            return LogsFile;
        }

        public string GetConfigFileLocation()
        {
            return ConfigFile;
        }

        public void LoadConfigFile()
        {
            if (!File.Exists(ConfigFile))
            {
                CreateDefaultConfigFile();
            }

            string json = File.ReadAllText(ConfigFile);
            config = JsonSerializer.Deserialize<Config>(json, options);

            //Verify case battle stuff are valid
            if (config.pvpStrategy != "HIGHEST_SUM" && config.pvpStrategy != "LOWEST_SUM" && config.pvpStrategy != "INVERSE_RANDOM_TICKET" && config.pvpStrategy != "RANDOM_TICKET" && config.pvpStrategy != "HIGHEST_BET_PAYOUT" && config.pvpStrategy != "LOWEST_BET_PAYOUT" && config.pvpStrategy != "HIGHEST_LAST_BET_PAYOUT" && config.pvpStrategy != "LOWEST_LAST_BET_PAYOUT")
            {
                config.pvpStrategy = "HIGHEST_SUM";
            }
            
            if(config.pvpNumOfTeams == 4)
            {
                config.pvpNumOfPlayers = 4;
            } else if (config.pvpNumOfTeams == 3)
            {
                if(config.pvpNumOfPlayers != 3 && config.pvpNumOfPlayers != 6)
                {
                    config.pvpNumOfPlayers = 3;
                }
            } else if (config.pvpNumOfTeams == 2)
            {
                if (config.pvpNumOfPlayers != 2 && config.pvpNumOfPlayers != 4 && config.pvpNumOfPlayers != 6)
                {
                    config.pvpNumOfPlayers = 2;
                }
            } else if (config.pvpNumOfTeams == 1)
            {
                if (config.pvpNumOfPlayers != 2 && config.pvpNumOfPlayers != 3 && config.pvpNumOfPlayers != 4)
                {
                    config.pvpNumOfPlayers = 2;
                }
            }

            if(config.pvpNumOfPlayers == config.pvpNumOfTeams)
            {
                config.pvpMode = "SINGLE";
                config.pvpNumOfTeams = 0;
            } else
            {
                config.pvpMode = "TEAM";
            }

            // Serialize the updated config back to JSON
            string updatedJson = JsonSerializer.Serialize(config, options);

            // Write the updated JSON string back to the config file
            File.WriteAllText(ConfigFile, updatedJson);
        }

        public void SetScheduleTime(string timestring)
        {
            TimeSpan ts = TimeSpan.Parse(timestring);
            userResetTimeSpan = ts;
        }

        public TimeSpan GetScheduleTime()
        {
            return userResetTimeSpan;
        }

        public Config GetConfigFile()
        {
            if(config == null)
            {
                LoadConfigFile();
            }

            return config;
        }

        public void CreateDefaultConfigFile()
        {
            string jsonString = JsonSerializer.Serialize(new Config(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, jsonString);
        }
    }
}
