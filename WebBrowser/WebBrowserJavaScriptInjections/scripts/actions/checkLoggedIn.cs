using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class checkLoggedIn : JavaScriptInjection
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
		  totalDeposit
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
}


function onPageLoad(callback) {
  if (document.readyState === 'complete') {
    // Page is already fully loaded
    callback();
  } else {
    // Wait for the load event
    window.addEventListener('load', callback);
  }
}

// Usage:
onPageLoad(function() {
  console.log(""The page is fully loaded (now or later)."");
  GetUserData();
});";
        }
    }
}
