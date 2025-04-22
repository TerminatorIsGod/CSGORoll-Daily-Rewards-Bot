using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class sellInventoryItems : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"async function createExchangeFromUserItems() {
  const endpoint = ""https://api.csgoroll.com/graphql"";

  // Step 1: Get current user ID
  const currentUserQuery = {
    operationName: null,
    variables: {},
    query: `
      query {
        currentUser {
          id
        }
      }
    `,
  };

  const currentUserRes = await fetch(endpoint, {
    method: ""POST"",
    headers: { ""Content-Type"": ""application/json"" },
    credentials: ""include"",
    body: JSON.stringify(currentUserQuery),
  });

  const currentUserJson = await currentUserRes.json();
  const userId = currentUserJson.data?.currentUser?.id;

  if (!userId) {
    console.error(""Failed to get user ID"");
    return;
  }

  // Step 2: Get user items
  const userItemsQuery = {
    operationName: ""UserItemList"",
    variables: {
      first: 50,
      userId: userId,
      orderBy: [""VALUE_DESC""],
      status: [""AVAILABLE""],
      includeMarketId: false,
    },
    query: `
      query UserItemList(
        $first: PaginationAmount!,
        $userId: ID!,
        $orderBy: [UserItemOrderBy!]!,
        $status: [UserItemStatus!]!,
      ) {
        userItems(
          first: $first,
          userId: $userId,
          orderBy: $orderBy,
          status: $status
        ) {
          edges {
            node {
              id
              status
              orderId
            }
          }
        }
      }
    `,
  };

  const userItemsRes = await fetch(endpoint, {
    method: ""POST"",
    headers: { ""Content-Type"": ""application/json"" },
    credentials: ""include"",
    body: JSON.stringify(userItemsQuery),
  });

  const userItemsJson = await userItemsRes.json();
  const items = userItemsJson.data?.userItems?.edges || [];

  if (items.length === 0) {
    console.log(userId);
    console.log(userItemsJson);
    console.warn(""No available user items found."");
    window.chrome.webview.postMessage({
      type: ""SellInventoryError"",
      payload: { message: ""No items found in inventory"" },
    });
    return;
  }

  const userItemIds = items.map(edge => edge.node.id);

  // Step 3: Create exchange using item IDs
  const createExchangeMutation = {
    operationName: ""CreateExchange"",
    variables: {
      input: {
        userItemIds: userItemIds,
        itemVariantIds: [],
      },
    },
    query: `
      mutation CreateExchange($input: CreateExchangeInput!) {
        createExchange(input: $input) {
          exchange {
            id
            itemsValue
            userItemsValue
            remainder
            profit
            currency
            newUserItems {
              edges {
                node {
                  id
                }
              }
            }
          }
        }
      }
    `,
  };

  const exchangeRes = await fetch(endpoint, {
    method: ""POST"",
    headers: { ""Content-Type"": ""application/json"" },
    credentials: ""include"",
    body: JSON.stringify(createExchangeMutation),
  });

  const exchangeJson = await exchangeRes.json();
  console.log(""Exchange created:"", exchangeJson.data?.createExchange?.exchange);
  window.chrome.webview.postMessage({
    type: ""SellInventory"",
    payload: exchangeJson.data?.createExchange?.exchange,
  });
}

// Call it
createExchangeFromUserItems();
";
        }
    }
}
