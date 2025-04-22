using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.functions
{
    class openBoxAsyncFunction : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"async function openBox(boxId, slots) {
const providedBoxMultiplierSlots = slots;//await fetchBoxSlots(boxId);

const body = {
operationName: ""OpenBox"",
variables: {
    input: {
    amount: 1,
    boxId: boxId,
    providedBoxMultiplierSlots,
    },
},
query: `
    mutation OpenBox($input: OpenBoxInput!) {
    openBox(input: $input) {
        box {
        id
        unopenedUserBoxesCount
        }
        boxOpenings {
        id
        createdAt
        ticketsWon
        boxItemId
        }
    }
    }
`,
};

fetch(""https://api.csgoroll.com/graphql"", {
method: ""POST"",
headers: {
    ""Content-Type"": ""application/json"",
},
credentials: ""include"", // Only needed if cookies/session auth is required
body: JSON.stringify(body),
})
.then((res) => res.json())
.then((data) => console.log(""Response:"", data))
.catch((err) => console.error(""Error:"", err));
}";
        }
    }
}
