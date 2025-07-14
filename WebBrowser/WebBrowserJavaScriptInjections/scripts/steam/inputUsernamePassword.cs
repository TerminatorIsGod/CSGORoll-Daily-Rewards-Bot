using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.steam
{
    class inputUsernamePassword : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null) // ""{args["code"]}""
        {
            if(args == null || args["username"] == null || args["password"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - inputUsernamePassword");
            }

            return $@"async function delay(time) {{
    return new Promise(resolve => setTimeout(resolve, time));
}}

async function setReactInputValue(selector, value) {{
    const input = document.querySelector(selector);
    if (input) {{
        const lastValue = input.value;
        input.value = value;

		await delay(100);

        const event = new Event('input', {{ bubbles: true }});
        
        // Hack for React: update the internal value tracker
        const tracker = input._valueTracker;
        if (tracker) {{
            tracker.setValue(lastValue);
        }}

        input.dispatchEvent(event);
    }}
}}

async function setDetails(){{
	await setReactInputValue('input[type=""text""]', '{args["username"]}');
	await setReactInputValue('input[type=""password""]', '{args["password"]}');
	const submitButton = document.querySelector('button[type=""submit""]');
	if (submitButton) {{
		submitButton.click();
	}}
}}

async function waitForSteamGuard() {{
	
	await delay(3000);
	
	const inputfieldcheck = document.querySelector('input[type=""password""]:not([id])');
    console.log(inputfieldcheck);
	
	if(inputfieldcheck) {{
		console.error(""Invalid username/password"");
		
		return; // add webview response
	}}
	
	while(true) {{
		var inputs = Array.from(document.querySelectorAll('input[type=""text""][role=""button""]'));

		if (inputs.length < 5) {{
			console.error(""Waiting for steam guard..."");
		}} else {{
			console.log(""steam guard ready."");
			window.chrome.webview.postMessage({{
				type: ""RequiresSteamGuardCode"",
				payload: {{ }}
			}});
			return;
		}}
		
		await delay(3000);
	}}
}}

async function signin() {{
	await setDetails();
	await waitForSteamGuard();
}}

signin();";
        }
    }
}
