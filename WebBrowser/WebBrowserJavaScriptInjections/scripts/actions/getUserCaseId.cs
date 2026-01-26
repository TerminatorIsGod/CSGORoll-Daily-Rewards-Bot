using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class getUserCaseId : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            if (args == null || args["risk"] == null || args["value"] == null || args["tier"] == null)
            {
                Form1._instance.printToConsole("FAILED TO GetJavaScript! Invalid args - getUserCaseId");
            }

            return $@"function parseSlug(slug) {{
  // Matches: corporal-iii-high-50
  const match = slug.match(/-(low|medium|high)-(\d+)$/);

  if (!match) return null;

  return {{
    value: match[1],        // low | medium | high
    risk: Number(match[2])  // 5 | 10 | 20 | 50 | 60
  }};
}}


async function getFilteredBoxes() {{
  const payload = {{
    operationName: ""TierRewardsBoxes"",
    query: `
      query TierRewardsBoxes(
        $rewardGroup: Int
        $free: Boolean
        $purchasable: Boolean
        $userProgressTierRequired: Int
        $tags: [String]
      ) {{
        boxes(
          rewardGroup: $rewardGroup
          free: $free
          purchasable: $purchasable
          userProgressTierRequired: $userProgressTierRequired
          tags: $tags
        ) {{
          edges {{
            node {{
              slug
              id
              name
              iconUrl
              locks {{
                chance
                walletType
              }}
            }}
          }}
        }}
      }}
    `,
    variables: {{
      free: true,
      purchasable: true,
      rewardGroup: 0,
      userProgressTierRequired: {args["tier"]}
    }}
  }};

  const res = await fetch(""https://api.csgoroll.com/graphql"", {{
    method: ""POST"",
    headers: {{ ""Content-Type"": ""application/json"" }},
    body: JSON.stringify(payload)
  }});

  const json = await res.json();

  if (json.errors) {{
    throw new Error(json.errors[0].message);
  }}

  const boxes = json.data.boxes.edges.map(e => e.node);

  const foundbox = 
    boxes.map(box => {{
      const parsed = parseSlug(box.slug);
      return parsed ? {{ ...box, ...parsed }} : null;
    }})
    .filter(Boolean)
    .filter(box => box.risk === {args["risk"]} && box.value === ""{args["value"]}"");
	
  return foundbox[0]?.id;
}}

async function getCaseID(){{
  const cid = await getFilteredBoxes();
  window.chrome.webview.postMessage({{
    type: ""caseIDToOpen"",
    payload: {{ message: cid }},
  }});
}}

getCaseID();";
        }
    }
}
