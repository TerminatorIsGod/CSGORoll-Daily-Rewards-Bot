using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class getShortestTime : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"function getBoxWithLowestLastPurchased() {
  const endpoint = ""https://api.csgoroll.com/graphql""; // Replace with the actual endpoint

  const body = {
    operationName: ""BoxGrid"",
    variables: {
      includeUserLastPurchasedAt: true,
      includeCreatorId: false,
      includeSlots: false,
      excludeBoxception: false,
      hydrateMostValuableOnly: false,
      free: true,
      purchasable: true,
      minLevelRequired: 2,
      orderBy: [""MIN_LEVEL_REQUIRED""],
      rewardGroup: 0,
      first: 100
    },
    query: `
      query BoxGrid(
        $free: Boolean,
        $purchasable: Boolean,
        $minLevelRequired: Int,
        $orderBy: [BoxOrderBy!],
        $rewardGroup: Int,
        $first: PaginationAmount
      ) {
        boxes(
          free: $free,
          purchasable: $purchasable,
          minLevelRequired: $minLevelRequired,
          orderBy: $orderBy,
          rewardGroup: $rewardGroup,
          first: $first
        ) {
          edges {
            node {
              id
              name
              slug
              levelRequired
              userLastPurchasedAt
            }
          }
        }
      }
    `
  };

  // Fetch request with credentials and headers
  fetch(endpoint, {
    method: ""POST"",
    headers: {
      ""Content-Type"": ""application/json"", // Important for sending JSON
      // Add additional headers here if required (e.g. Authorization)
    },
    credentials: ""include"", // Ensure cookies/session are sent along with the request
    body: JSON.stringify(body), // The body of the request with the GraphQL query and variables
  })
  .then(res => res.json()) // Parse the response JSON
  .then(data => {
    console.log(""Full API response:"", data); // Log the full response for debugging

    // Process the data to find the box with the lowest userLastPurchasedAt
    const boxes = data?.data?.boxes?.edges || [];

    if (boxes.length === 0) {
      console.warn(""No boxes found."");
      return null;
    }

    // Filter out boxes where 'userLastPurchasedAt' is null or empty
    const validBoxes = boxes.filter(box => {
      const userLastPurchasedAt = box.node.userLastPurchasedAt;
      return userLastPurchasedAt && userLastPurchasedAt.trim() !== """";
    });

    if (validBoxes.length === 0) {
      console.warn(""No valid boxes with 'userLastPurchasedAt' found."");
      return null;
    }

    // Sort the valid boxes by 'userLastPurchasedAt'
    validBoxes.sort((a, b) => new Date(a.node.userLastPurchasedAt) - new Date(b.node.userLastPurchasedAt));

    // Return the box with the lowest (earliest) 'userLastPurchasedAt'
    const boxWithLowestPurchase = validBoxes[0].node;

    console.log(""Box with lowest userLastPurchasedAt:"", boxWithLowestPurchase);
    window.chrome.webview.postMessage({
        type: ""ShortestTime"",
        payload: boxWithLowestPurchase
    });
    return boxWithLowestPurchase;
  })
  .catch(err => {
    console.error(""Error fetching data:"", err);
  });
}

getBoxWithLowestLastPurchased();
";
        }
    }
}
