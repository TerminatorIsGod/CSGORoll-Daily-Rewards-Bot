using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.commands
{
    interface WebBrowserCommand
    {
        void executeCommand(string command, string[] args);
    }
}
