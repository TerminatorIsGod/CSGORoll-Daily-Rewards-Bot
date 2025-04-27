using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class createPvPGame : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return $@"const boxesVar = {args["boxes"]};
const strategyVar = ""{args["strategy"]}"";
const numberOfPlayersVar = {int.Parse(args["playercount"])};
const pvpmode = ""{args["mode"]}"";
const numberOfTeamsVar = {int.Parse(args["teamcount"])};
const numberOFPlayersInTeamVar = {int.Parse(args["teamplayerscount"])};

async function createPvpGame() {{
  const body = {{
    operationName: ""CreatePvpGame"",
    variables: {{
      input: {{
        type: ""BOX"",
        amount: 0,
        userItemIds: [],
        boxes: boxesVar,
        selection: 1,
        isPrivate: false,
        strategy: strategyVar,
        fastMode: false,
        brandSpin: true,
        enableEarlyCashout: false,
        mode: pvpmode,
        numberOfPlayers: numberOfPlayersVar,
        numberOfPlayersInTeam: numberOFPlayersInTeamVar,
        multiplierMode: ""PVP"",
        numberOfTeams: numberOfTeamsVar,
        autoJoinBots: true,
        teamSelection: 0
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

  try {{
    console.log(""fetching..."");
    const res = await fetch(""https://api.csgoroll.com/graphql"", {{
      method: ""POST"",
      headers: {{
        ""Content-Type"": ""application/json"",
      }},
      credentials: ""include"",
      body: JSON.stringify(body),
    }});

    console.log(""fetching... 2"");

    const data = await res.json();
    console.log(""Battle created: "", data);
    console.log(""Sent data: "", data.data);

    window.chrome.webview.postMessage({{
      type: ""PvpbattleCreated"",
      payload: data.data // this matches your C# class
    }});

  }} catch (err) {{
    console.log(""Failed creating case battle"");
    window.chrome.webview.postMessage({{
      type: ""PvpbattleCreatedError"",
      payload: {{ message: err.message }}
    }});
  }}
}}

// Call the function
createPvpGame();
";
        }
    }
}
