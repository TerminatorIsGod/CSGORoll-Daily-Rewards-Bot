using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.actions
{
    class setPromoCode : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"async function applyPromoCode(code) {
  const body = {
    operationName: ""applyPromoCodeTimer"",
    variables: {
      input: {
        code: code
      }
    },
    query: `
      mutation applyPromoCodeTimer($input: ApplyPromoCodeInput!) {
        applyPromoCodeTimer(input: $input) {
          ...PromoCodeTimer
          __typename
        }
      }

      fragment PromoCodeTimer on AppliedPromoCodeTimer {
        promoCode
        secondsLeft
        status
        __typename
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

    const data = await res.json();
    console.log(""Response:"", data);

    window.chrome.webview.postMessage({
      type: ""PromoCodeSuccess"",
      payload: data
    });
  } catch (err) {
    console.error(""Error:"", err);

    window.chrome.webview.postMessage({
      type: ""PromoCodeError"",
      payload: {
        message: err.message || ""Unknown error""
      }
    });
  }
}
applyPromoCode(""ABLUE"");";
        }
    }
}
