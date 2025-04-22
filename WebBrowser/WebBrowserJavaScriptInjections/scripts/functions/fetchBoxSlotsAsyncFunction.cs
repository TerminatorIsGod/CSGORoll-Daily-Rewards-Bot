using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts
{
    class fetchBoxSlotsAsyncFunction : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null) //needs the variable 'const boxId = "Qm94OjI1MDkw";'
        {
            return @"async function fetchBoxSlots(boxId) {
    const res = await fetch(""https://api.csgoroll.com/graphql"", {
    method: ""POST"",
    headers: {
        ""Content-Type"": ""application/json"",
    },
    body: JSON.stringify({
        query: `
        query BoxSlots($boxId: ID!) {
            box(id: $boxId) {
            slots {
                item {
                id
                value
                }
                balance
            }
            }
        }
        `,
        variables: { boxId },
    }),
    });

    const json = await res.json();
    const slots = json.data.box.slots;

    // Convert slots to providedBoxMultiplierSlots format
    const providedBoxMultiplierSlots = slots.map((slot) => ({
    balance: slot.balance,
    item: {
        id: slot.item.id,
        value: slot.item.value,
    },
    }));

    return providedBoxMultiplierSlots;
}";
        }
    }
}
