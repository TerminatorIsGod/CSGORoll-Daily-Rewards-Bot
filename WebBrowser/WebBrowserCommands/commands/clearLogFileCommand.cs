using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBrowser.commands;

namespace WebBrowser.WebBrowserCommands.commands
{
    class clearLogFileCommand : WebBrowserCommand
    {
        public void executeCommand(string command, string[] args)
        {
            Form1._instance.clearConsoleTextFile();
        }
    }
}
