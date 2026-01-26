using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public int level { get; set; }
        public string steamId { get; set; }
        public string dailyFreeTimeSlot { get; set; }
        public List<UserWallet> wallets { get; set; }
        public List<UserProgresses> userProgresses { get; set; }
    }

    class UserWallet
    {
        public string id { get; set; }
        public string name { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
    }

    class UserProgresses
    {
        public string id { get; set; }
        public string type { get; set; }
        public int xp { get; set; }
        public UserProgressTier userProgressTier { get; set; }
        public UserProgressTier nextTier { get; set; }
    }

    class UserProgressTier
    {
        public string id { get; set; }
        public string name { get; set; }
        public int order { get; set; }
        public int xpRequired { get; set; }
    }
}
