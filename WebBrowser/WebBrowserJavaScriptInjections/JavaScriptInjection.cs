using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections
{
    abstract class JavaScriptInjection
    {
        public static JavaScriptInjection _instance;

        public JavaScriptInjection()
        {
            _instance = this;
        }

        public virtual string GetJavaScript(Dictionary<string, string> args = null)
        {
            return "";
        }
    }
}
