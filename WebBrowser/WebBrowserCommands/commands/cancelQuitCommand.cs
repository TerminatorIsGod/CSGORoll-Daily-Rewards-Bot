using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.commands;

namespace WebBrowser.WebBrowserCommands.commands
{
    class cancelQuitCommand : WebBrowserCommand
    {
        public void executeCommand(string command, string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "true":
                        Form1._instance.disableAutoQuit = true;
                        return;
                    case "false":
                        Form1._instance.disableAutoQuit = false;
                        return;
                    default:
                        Form1._instance.disableAutoQuit = !Form1._instance.disableAutoQuit;
                        return;
                }
                //Form1._instance.disableAutoQuit = args[0];
            }

            Form1._instance.disableAutoQuit = !Form1._instance.disableAutoQuit;

            Form1._instance.printToConsole("Cancel Auto Quit: " + Form1._instance.disableAutoQuit);
        }
    }
}
