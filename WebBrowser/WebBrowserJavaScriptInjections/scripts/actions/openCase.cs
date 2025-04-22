using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.functions;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class openCase : JavaScriptInjection
    {
        /// <summary>
        /// Generates the JavaScript injection code for opening a case.
        /// </summary>
        /// <param name="args">
        /// This action requires to be passed a boxId like this: args["boxId] = "aa8dasdsdf1mas"
        /// </param>
        /// <returns>
        /// A full JavaScript string that fetches slots and opens a case.
        /// </returns>
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            if (args == null || args["boxId"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - openCase");
            }

            string fetchBoxSlotsScript = new fetchBoxSlotsAsyncFunction().GetJavaScript();
            string openBoxScript = new openBoxAsyncFunction().GetJavaScript();

            return $@"const boxId = ""{args["boxId"]}"";
{fetchBoxSlotsScript}

{openBoxScript}

async function openCase(boxId){{
	const slots = await fetchBoxSlots(boxId);
	openBox(boxId, slots);
}}
openCase(boxId);";
        }
    }
}
