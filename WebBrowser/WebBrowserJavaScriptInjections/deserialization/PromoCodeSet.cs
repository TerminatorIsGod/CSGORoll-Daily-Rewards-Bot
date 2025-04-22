using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    class PromoCodeSet
    {
        public promoCodeSetData data { get; set; }
    }

    public class promoCodeSetData
    {
        public promoCodeSetApplyPromoCodeTimer applyPromoCodeTimer { get; set; }
    }

    public class promoCodeSetApplyPromoCodeTimer
    {
        public string promoCode { get; set; }
        public int secondsLeft { get; set; }
        public string status { get; set; }
        public string __typename { get; set; }
    }
}
