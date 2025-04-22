using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    class WebMessage
    {
        public string type { get; set; }
        public JsonElement payload { get; set; }
    }
}
