using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class getUserData : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"async function getCurrentUser() {
  const body = {
    operationName: null,
    variables: {},
    query: `
      query {
        currentUser {
          id
          name
          avatar
          level
          steamId
          dailyFreeTimeSlot
          wallets {
            id
            name
            amount
            currency
          }
          __typename
        }
      }
    `
  };

  try {
    const res = await fetch(""https://api.csgoroll.com/graphql"", {
      method: ""POST"",
      headers: {
        ""Content-Type"": ""application/json"",
      },
      credentials: ""include"",
      body: JSON.stringify(body),
    });

    const json = await res.json();
    return json.data.currentUser; // returns the actual user object

  } catch (err) {
    console.error(""Error fetching user:"", err);
    return null;
  }
}

async function GetUserData(){
	const user = await getCurrentUser();
	console.log(""Current user:"", user);
    if(user == null) { 
        console.log(""User is null"");
        window.chrome.webview.postMessage({
            type: ""UserDataNull"",
            payload: null
        });
    } else {
        console.log(""User is not null"");
        window.chrome.webview.postMessage({
            type: ""UserData"",
            payload: user
        });
    }
}

GetUserData();";
        }
    }
}
