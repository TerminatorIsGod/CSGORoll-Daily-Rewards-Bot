using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.functions;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class openCaseFull : JavaScriptInjection
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

            return $@"window.boxId = ""{args["boxId"]}"";

async function delay(time) {{
  return new Promise(resolve => setTimeout(resolve, time));
}}

async function fetchBoxSlots(boxId) {{
  try {{
    const res = await fetch(""https://api.csgoroll.com/graphql"", {{
      method: ""POST"",
      credentials: ""include"",
      headers: {{
        ""Content-Type"": ""application/json"",
        ""accept"": ""application/json, text/plain, */*"",
        ""accept-encoding"": ""gzip, deflate, br, zstd"",
        ""accept-language"": ""en-US,en;q=0.9"",
        ""apollographql-client-id"": ""www-csgoroll-0"",
        ""apollographql-client-name"": ""www-csgoroll"",
        ""apollographql-client-version"": ""v241"",
        ""x-apollo-operation-name"": ""BoxSlots"",
        ""User-Agent"": navigator.userAgent,
        ""Referer"": ""https://www.csgoroll.com/"",
        ""Origin"": ""https://www.csgoroll.com"",
        ""sec-ch-ua"": `""Not A(Brand"";v=""8"", ""Chromium"";v=""132"", ""Opera"";v=""117""`,
        ""sec-ch-ua-mobile"": ""?0"",
        ""sec-ch-ua-platform"": `""Windows""`,
        ""sec-fetch-dest"": ""empty"",
        ""sec-fetch-mode"": ""cors"",
        ""sec-fetch-site"": ""same-site"",
        ""ngsw-bypass"": ""true"",
      }},
      body: JSON.stringify({{
        query: `
          query BoxSlots($boxId: ID!) {{
            box(id: $boxId) {{
              slots {{
                item {{
                  id
                  value
                }}
                balance
              }}
            }}
          }}
        `,
        variables: {{ boxId }},
      }}),
    }});

    const json = await res.json();

    if (!json || json.errors || !json.data || !json.data.box) {{
      console.error(""Invalid response:"", json);
      throw new Error(""Failed to fetch valid box data."");
    }}

    const slots = json.data.box.slots;

    const providedBoxMultiplierSlots = slots.map((slot) => ({{
      balance: slot.balance,
      item: slot.item ? {{
        id: slot.item.id,
        value: slot.item.value,
      }} : null,
    }}));

    // Send the fetched slots back
    return providedBoxMultiplierSlots;
  }} catch (err) {{
    console.error(""Error fetching box slots:"", err);
    window.chrome.webview.postMessage({{
      type: ""BoxSlotsError"",
      payload: {{ message: err.message }},
    }});
    return [];
  }}
}}

async function openBox(boxId, slots) {{
  const body = {{
    operationName: ""OpenBox"",
    variables: {{
      input: {{
        amount: 1,
        boxId: boxId,
        providedBoxMultiplierSlots: slots,
      }},
    }},
    query: `
      mutation OpenBox($input: OpenBoxInput!) {{
        openBox(input: $input) {{
          box {{
            id
            unopenedUserBoxesCount
          }}
          boxOpenings {{
            id
            createdAt
            ticketsWon
            boxItemId
            profit
            balance
            roll{{
                value
            }}
            userItem {{
                consumeValue
                consumeValueInUserCurrency
                itemVariant{{
                    name
                    brand
                    value
                    iconUrl
                }}
            }}
          }}
        }}
      }}
    `,
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
    console.log(""Box opened:"", data);

    window.chrome.webview.postMessage({{
      type: ""BoxOpened"",
      payload: data,
    }});
  }} catch (err) {{
    console.error(""Error opening box:"", err);
    window.chrome.webview.postMessage({{
      type: ""BoxSlotsError"",
      payload: {{ message: err.message }},
    }});
  }}
}}

async function openCase(boxId) {{
  console.log(""openCase"");
  const slots = await fetchBoxSlots(boxId);
  delay(1000);
  console.log(""fetchboxslots done"");
  console.log(slots);
  await openBox(boxId, slots);
  console.log(""openBox done"");
}}

openCase(boxId);
";
        }
    }
}
