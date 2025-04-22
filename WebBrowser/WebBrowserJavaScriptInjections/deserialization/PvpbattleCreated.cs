using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    class PvpbattleCreated
    {
        public PvpbattleCreatePvpGame createPvpGame { get; set; }
    }

    class PvpbattleCreatePvpGame
    {
        public PvpBattlePvpGame pvpGame { get; set; }
    }

    class PvpBattlePvpGame
    {
        public string id { get; set; }
        public string type { get; set; }
    }
}
