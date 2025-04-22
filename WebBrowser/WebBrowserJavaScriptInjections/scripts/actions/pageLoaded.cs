using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class pageLoaded : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"async function delay(time) {
  return new Promise(resolve => setTimeout(resolve, time));
}

function onPageLoad(callback) {
  if (document.readyState === 'complete') {
    // Page is already fully loaded
    callback();
  } else {
    // Wait for the load event
    window.addEventListener('load', callback);
  }
}

// Usage
onPageLoad(async function () {
  console.log(""The page is fully loaded."");

  await delay(100); // Delay to ensure things settle

  window.chrome.webview.postMessage({
    type: ""LoadStatus"",
    payload: { message: ""success"" }
  });
});
";
        }
    }
}
