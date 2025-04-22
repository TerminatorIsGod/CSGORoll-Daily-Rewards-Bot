using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.steam
{
    class pressSignInButton : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"if (document.readyState === 'complete') {
  // Page is already fully loaded
  document.getElementById('imageLogin').click();
} else {
  // Wait for the load event
  window.addEventListener('load', function() {
    document.getElementById('imageLogin').click();
});
}
";
        }
    }
}
