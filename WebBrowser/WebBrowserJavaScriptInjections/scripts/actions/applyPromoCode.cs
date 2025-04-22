using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.functions;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class applyPromoCode : JavaScriptInjection
    {
        /// <summary>
        /// Generates the JavaScript injection code for opening a case.
        /// </summary>
        /// <param name="args">
        /// This action requires to be passed a promocode like this: ["code"] = "ABLUE"
        /// </param>
        /// <returns>
        /// A full JavaScript string that fetches slots and opens a case.
        /// </returns>
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            if(args == null || args["code"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - applyPromoCode");
            }

            string applyPromocodeFunc = new applyPromoCodeFunction().GetJavaScript();

            return $@"const promocode = ""{args["code"]}"";
{applyPromocodeFunc}

applyPromoCode(promocode);";
        }
    }
}
