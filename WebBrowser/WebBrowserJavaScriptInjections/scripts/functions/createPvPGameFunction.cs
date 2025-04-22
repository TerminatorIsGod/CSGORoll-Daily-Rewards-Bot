using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.functions
{
    class createPvPGameFunction : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return $@"
async function createPvpGame() {{
  const body = {{
    operationName: ""CreatePvpGame"",
    variables: {{
      input: {{
        type: ""BOX"",
        amount: 0,
        userItemIds: [],
        boxes: [
          {{
            boxId: ""Qm94OjI1MDM2"",
            roundNumber: 1
          }}
        ],
        selection: 1,
        isPrivate: false,
        strategy: ""LOWEST_SUM"",
        fastMode: false,
        brandSpin: true,
        enableEarlyCashout: false,
        mode: ""SINGLE"",
        numberOfPlayers: 3,
        multiplierMode: ""PVP"",
        numberOfTeams: 0,
        autoJoinBots: true
      }}
    }},
    query: `
      mutation CreatePvpGame($input: CreatePvpGameInput!) {{
        createPvpGame(input: $input) {{
          pvpGame {{
            id
            type
            __typename
          }}
          __typename
        }}
      }}
    `
  }};

  fetch(""https://api.csgoroll.com/graphql"", {{
    method: ""POST"",
    headers: {{
      ""Content-Type"": ""application/json"",
    }},
    credentials: ""include"", // Make sure you're logged in
    body: JSON.stringify(body),
  }})
    .then((res) => res.json())
    .then((data) => console.log(""Response:"", data))
    .catch((err) => console.error(""Error:"", err));
}}";
        }
    }
}
