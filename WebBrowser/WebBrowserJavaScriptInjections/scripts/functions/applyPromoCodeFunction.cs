using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.functions
{
    class applyPromoCodeFunction : JavaScriptInjection
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

  fetch(""https://api.csgoroll.com/graphql"", {
    method: ""POST"",
    headers: {
      ""Content-Type"": ""application/json"",
    },
    credentials: ""include"", // If the site uses cookies/session
    body: JSON.stringify(body),
  })
    .then((res) => res.json())
    .then((data) => console.log(""Response:"", data))
    .catch((err) => console.error(""Error:"", err));
}";
        }
    }
}
