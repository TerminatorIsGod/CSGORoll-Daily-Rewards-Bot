using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.deserialization
{
    public class Exchange
    {
        public string Id { get; set; }
        public decimal ItemsValue { get; set; }
        public decimal UserItemsValue { get; set; }
        public decimal Remainder { get; set; }
        public decimal Profit { get; set; }
        public string Currency { get; set; }
        public exchangeNewUserItems NewUserItems { get; set; }
    }

    public class exchangeNewUserItems
    {
        public List<exchangeEdge> Edges { get; set; }
    }

    public class exchangeEdge
    {
        public exchangeNode Node { get; set; }
    }

    public class exchangeNode
    {
        public string Id { get; set; }
    }
}
