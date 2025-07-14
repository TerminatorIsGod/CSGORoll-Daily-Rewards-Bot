using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.steam
{
    class inputSteamGuardCode : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            if (args == null || args["steamcode"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - inputSteamGuardCode");
            }

            return $@"function fillTextInputsWithString(str) {{
    if (str.length !== 5) {{
        console.error(""Input string must be exactly 5 characters long."");
        return;
    }}

    const inputs = Array.from(document.querySelectorAll('input[type=""text""][role=""button""]'));

    if (inputs.length < 5) {{
        console.error(""Not enough input fields found. Expected 5."");
        return;
    }}

    inputs.slice(0, 5).forEach((input, index) => {{
        const char = str[index];
        const lastValue = input.value;
        input.value = char;

        // React/Framework compatibility
        const event = new Event('input', {{ bubbles: true }});
        const tracker = input._valueTracker;
        if (tracker) {{
            tracker.setValue(lastValue);
        }}

        input.dispatchEvent(event);
    }});
}}

fillTextInputsWithString(""{args["steamcode"]}"");";
        }
    }
}
