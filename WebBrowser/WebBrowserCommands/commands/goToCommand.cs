using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.commands;

namespace WebBrowser.WebBrowserCommands.commands
{
    class goToCommand : WebBrowserCommand
    {
        public void executeCommand(string command, string[] args)
        {
            if (args.Length < 1)
            {
                //Didn't provide a URL
                Form1._instance.printToConsole("Invalid command arguments! Usage: goto <url>");
            }
            else
            {
                Uri result = null;
                if (Uri.TryCreate(args[0], UriKind.Absolute, out result))
                {
                    Form1._instance.GetWebviewObj().Source = result;
                }
            }
        }
    }
}
