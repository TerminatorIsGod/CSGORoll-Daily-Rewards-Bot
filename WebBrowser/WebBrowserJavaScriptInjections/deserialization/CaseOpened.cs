using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    class CaseOpened
    {
        public caseOpenedData data { get; set; }
        public List<GraphQLError> errors { get; set; }
    }

    public class caseOpenedData
    {
        public caseOpenedOpenBox openBox { get; set; }
    }

    public class caseOpenedOpenBox
    {
        public caseOpenedBox box { get; set; }
        public List<caseOpenedBoxOpening> boxOpenings { get; set; }
    }

    public class caseOpenedBox
    {
        public string id { get; set; }
        public int unopenedUserBoxesCount { get; set; }
    }

    public class caseOpenedBoxOpening
    {
        public string id { get; set; }
        public DateTime createdAt { get; set; }
        public int ticketsWon { get; set; }
        public string boxItemId { get; set; }
        public double profit { get; set; }
        public double balance { get; set; }
        public caseOpenedRoll roll { get; set; }
        public caseOpenedUserItem userItem { get; set; }
    }

    public class caseOpenedUserItem
    {
        public double consumeValue { get; set; }
        public double consumeValueInUserCurrency { get; set; }
        public caseOpenedItemVariant itemVariant { get; set; }
    }

    public class caseOpenedItemVariant
    {
        public string name { get; set; }
        public string brand { get; set; }
        public double value { get; set; }
        public string iconUrl { get; set; }
    }

    public class caseOpenedRoll
    {
        public int value { get; set; }
    }

    public class GraphQLError
    {
        public string message { get; set; }
        public List<GraphQLErrorLocation> locations { get; set; }
        public List<string> path { get; set; }
        public GraphQLErrorExtensions extensions { get; set; }
    }

    public class GraphQLErrorLocation
    {
        public int line { get; set; }
        public int column { get; set; }
    }

    public class GraphQLErrorExtensions
    {
        public string code { get; set; }
    }
}
