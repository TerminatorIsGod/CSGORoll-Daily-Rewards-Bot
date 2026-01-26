using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class lockpickCase : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            if (args == null || args["boxId"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - lockpickCase");
            }

            return $@"async function picklockBox(boxId) {{
  const body = {{
    operationName: ""PicklockBox"",
    variables: {{
      input: {{
        boxId: boxId
      }}
    }},
    query: `
      mutation PicklockBox($input: PicklockBoxInput!) {{
        picklockBox(input: $input) {{
          picklock {{
            ...PicklockBoxFragment
            __typename
          }}
          __typename
        }}
      }}

      fragment PicklockBoxFragment on Picklock {{
        boxId
        success
        validUntil
        chance
        box {{
          name
          slug
          iconUrl
          __typename
        }}
        __typename
      }}
    `
  }};

  try {{
    const res = await fetch(""https://api.csgoroll.com/graphql"", {{
      method: ""POST"",
      headers: {{
        ""Content-Type"": ""application/json"",
      }},
      credentials: ""include"",
      body: JSON.stringify(body),
    }});

    const data = await res.json();
    console.log(""Response:"", data);

    // IMPORTANT: always check errors first
    if (data.errors) {{
      throw new Error(data.errors[0].message);
    }}

    window.chrome.webview.postMessage({{
          type: ""lockpickedCaseSuccess"",
          payload: {{ success: true }},
        }});
    return data.data.picklockBox.picklock;

  }} catch (err) {{
    console.error(""Error picking lock:"", err);
    window.chrome.webview.postMessage({{
          type: ""lockpickedCaseError"",
          payload: {{ message: err.message }},
        }});
    return null;
  }}
}}

picklockBox(""{args["boxId"]}"");";
        }
    }
}
