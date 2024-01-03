using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace WebBrowser
{
    public partial class Form1 : Form
    {
        public static Form1 _instance;

        string taskName = "csgoRollAutoDaily";

        public bool disableAutoQuit = false;

        bool showInitalizationScreen = false;

        bool quittingApplication = false;

        int loadPageDelayMiliSec = 6000;

        string filePathFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CSGORollDailyCollector");

        //private bool firstTimeLaunched = false;

        Dictionary<string, WebBrowserCommand> webBrowserCommands = new Dictionary<string, WebBrowserCommand>();

        public Form1()
        {
            InitializeComponent();

            _instance = this;

            this.FormClosing += Form1_FormClosing;

            //Register Commands
            insertWebBrowserCommand("autoQuit", new cancelQuitCommand());
            insertWebBrowserCommand("goTo", new goToCommand());

            AutoCompleteStringCollection autoCompleteCollection = new AutoCompleteStringCollection();
            autoCompleteCollection.AddRange(webBrowserCommands.Keys.ToArray());
            textBox1.AutoCompleteCustomSource = autoCompleteCollection;

            if (!File.Exists(filePathFolder + "Initalized.Egario"))
            {
                showInitalizationScreen = true; 
            } else
            {
                textBox2.Visible = true;
            }
            printToConsole($"{filePathFolder}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!quittingApplication)
            {
                // Ask the user if they really want to close the form
                DialogResult result = MessageBox.Show("Manually closing the application is not recommend! The program should automatically close itself after a few minutes.", "Are you sure you want to quit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // If the user clicks No, cancel the closing event
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        void insertWebBrowserCommand(string command, WebBrowserCommand commandObject)
        {
            webBrowserCommands.Add(command.ToLower(), commandObject);
        }

        WebBrowserCommand getWebBrowserCommand(string command)
        {
            WebBrowserCommand commandObject = null;
            webBrowserCommands.TryGetValue(command.ToLower(), out commandObject);
            return commandObject;
        }

        public void printToConsole(String text)
        {
            richTextBox1.AppendText("\r\n" + text);
            richTextBox1.ScrollToCaret();
        }

        public void setURLTextbox(String url)
        {
            richTextBox2.Text = "URL: " + url;
        }

        private async Task<TimeSpan> getTimeLeft()
        {
            try
            {
                await DelayAsyncSec(1);
                string script = "function getTime(){\r\nconst elements = document.querySelectorAll('cw-countdown span:not(.text-warning)');\r\n\r\nfor (const element of elements) {\r\n\r\n  if (element.innerText.trim() === '' || !/\\d/.test(element.innerText)) {\r\n\r\n  } else {\r\n    console.log(element.innerText);\r\n    return element.innerText;\r\n\r\n    break;\r\n  }\r\n}\r\n}\r\n\r\ngetTime();";
                string timeLeft = await webView21.ExecuteScriptAsync(script);
                printToConsole(timeLeft);
                return ParseDurationString(timeLeft);
            }
            catch (Exception ex)
            {
                printToConsole(ex.ToString());
            }

            return new TimeSpan(0,0,0);
        }

        private void updateTaskSchedularTask(TimeSpan timeSpanToAdd)
        {
            printToConsole("Updating task");
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskName);

                //Gets the time left till able to open and subtracts 1 minute
                TimeSpan timespan = timeSpanToAdd.Subtract(new TimeSpan(0, 1, 0));

                if (task != null)
                {
                    //Clear previous daily trigger so it can trigger in the same day
                    TaskDefinition taskDef = task.Definition;
                    taskDef.Triggers.Clear();

                    DailyTrigger dailyTrigger = new DailyTrigger();
                    dailyTrigger.DaysInterval = 1;
                    dailyTrigger.StartBoundary = DateTime.Now.Add(timespan);

                    taskDef.Triggers.Add(dailyTrigger);

                    taskDef.Actions.Clear();
                    taskDef.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location));

                    // Save the changes
                    task.RegisterChanges();
                } else
                {
                    createTaskSchedularTask(ts, timespan);
                }
            }
        }

        private void createTaskSchedularTask(TaskService ts, TimeSpan timeSpanToAdd)
        {
            printToConsole("Creating task");
            TaskDefinition taskDef = ts.NewTask();
            taskDef.RegistrationInfo.Description = "CSGORoll Daily Collector.";

            DailyTrigger dailyTrigger = new DailyTrigger();
            taskDef.Settings.WakeToRun = true;
            taskDef.Settings.DisallowStartIfOnBatteries = false;
            taskDef.Settings.RunOnlyIfIdle = false;
            dailyTrigger.DaysInterval = 1;
            dailyTrigger.StartBoundary = DateTime.Now.Add(timeSpanToAdd);

            taskDef.Triggers.Add(dailyTrigger);

            taskDef.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location));

            ts.RootFolder.RegisterTaskDefinition(taskName, taskDef);
        }



        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (showInitalizationScreen)
            {
                showInitalizationScreen = false;
                showInitalizationPage();
                return;
            }

            setURLTextbox(webView21.Source.ToString());
            printToConsole(webView21.Source.ToString());
            if (webView21.Source.ToString().Contains("/csgo/p2p"))
            {
                //check if logged in, if not go to rewards
                await webView21.ExecuteScriptAsync("var element = document.querySelector('[data-test=\\\"auth-login-btn\\\"]');\r\nif (element != null) { element.click(); }\r\nelse { window.location.href = 'https://www.csgoroll.com/en/boxes/world/daily-free'; }");
            } else if (webView21.Source.ToString().Contains("steamcommunity.com")) {
                await webView21.ExecuteScriptAsync("var signInButton = document.getElementById('imageLogin');\r\nif (signInButton) {\r\n    signInButton.click();\r\n} else {\r\n    console.error('Sign In button not found.');\r\n}");
            } else if (webView21.Source.ToString().Contains("/daily-free"))
            {
                // Wait for page to load
                await DelayAsync(loadPageDelayMiliSec);

                //Check if still on correct page
                if (!webView21.Source.ToString().Contains("/daily-free"))
                {
                    return;
                }

                TimeSpan ts = await getTimeLeft();
                printToConsole($"Time Seconds Left: {ts.TotalSeconds}");
                if (ts.TotalSeconds > 0)
                {
                    thereIsATimer(ts);
                } else
                {
                    openAllCases();
                }
            } else if (webView21.Source.ToString().Contains("/summary"))
            {
                setAffiliateCode();
            }
            
        }

        private async void openAllCases()
        {
            await webView21.ExecuteScriptAsync("function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function executeStuff() {\r\n    await delay(3000);\r\n    var elements = document.getElementsByClassName('open-btn');\r\n    if (elements.length == 0) {\r\n        console.log('Failed to find button!');\r\n    } else {\r\n        console.log('Found button!');\r\n        for (var i = 0; i < elements.length; i++) {\r\n            if (!elements[i].classList.contains('mat-button-disabled')) {\r\n                elements[i].click();\r\n\r\n                var elementsToOpenCase = document.getElementsByClassName('open-btn');\r\n                for (var j = 0; j < elementsToOpenCase.length; j++) {\r\n                    if (!elementsToOpenCase[j].classList.contains('mat-flat-button')) {\r\n                        console.log('Found open case button!');\r\n                        await delay(2000);\r\n                        elementsToOpenCase[j].click();\r\n                    }\r\n                }\r\n\r\n                // Case opened, sell item sell-btn click twice with a small delay\r\n                await delay(10000);\r\n                var sellElements = document.getElementsByClassName('sell-btn');\r\n                for (var k = 0; k < sellElements.length; k++) {\r\n                    sellElements[k].click();\r\n                }\r\n\r\n                await delay(1000);\r\n\r\n                for (var l = 0; l < sellElements.length; l++) {\r\n                    sellElements[l].click();\r\n                }\r\n\r\n                await delay(1000);\r\n\r\n                var closeElements = document.getElementsByClassName('close');\r\n                for (var m = 0; m < closeElements.length; m++) {\r\n                    closeElements[m].click();\r\n                }\r\n            }\r\n        }\r\n    }\r\n}\r\n\r\nexecuteStuff();");
            await DelayAsyncSec(160);
            //await setAffiliateCode();
            TimeSpan ts = await getTimeLeft();
            thereIsATimer(ts);
        }

        private void quitWebBrowser()
        {
            if (!disableAutoQuit)
            {
                quittingApplication = true;
                Application.Exit();
            }
        }

        private async System.Threading.Tasks.Task goToProfilePage()
        {
            webView21.ExecuteScriptAsync("function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nfunction goToProfilePage() {\r\n\r\n    var profileButtonElement = document.querySelector('button[data-test=\"user-avatar\"]');\r\n    profileButtonElement.click();\r\n\r\n  " + $"  delay({loadPageDelayMiliSec});" + "\r\n\r\n    var spanElement = document.querySelector('span[data-test=\"username\"]');\r\n    spanElement.click();\r\n\r\n    var linkElement = spanElement.closest('a');\r\n    linkElement.click();\r\n}\r\n\r\ngoToProfilePage();");
            await DelayAsync(loadPageDelayMiliSec + 1000);
        }

        private async System.Threading.Tasks.Task setAffiliateCode()
        {
            webView21.ExecuteScriptAsync("function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function setCode() {\r\n " +   $"await delay({loadPageDelayMiliSec});" + "\r\n    var affiliateCodeTextbox = document.querySelector('[formcontrolname=\"code\"]');\r\n    affiliateCodeTextbox.value = \"EGAR.IO\";\r\n    await delay(1000);\r\n    var applyAffiliateCodeBtn = document\r\n    var applyButton = document.querySelector('button.mat-focus-indicator.mat-button-3d.mat-flat-button.mat-button-base.mat-accent.mat-button-lg.w-100.ng-star-inserted');\r\n    var inputEvent = new Event('input', {\r\n        bubbles: true,\r\n        cancelable: true,\r\n    });\r\n\r\n    affiliateCodeTextbox.dispatchEvent(inputEvent);\r\n    applyButton.click();\r\n}\r\n\r\nsetCode();");
            await DelayAsync(loadPageDelayMiliSec + 1000);
        }

        private async void thereIsATimer(TimeSpan ts)
        {
            if(ts.TotalSeconds >= 300)
            {
                //Reschedule
                printToConsole("Timer is greater than 300 seconds, rescheduling...");
                updateTaskSchedularTask(ts);
                await goToProfilePage();
                await DelayAsyncSec(5);
                //Quit
                quitWebBrowser();
            } else
            {
                await DelayAsyncSec(ts.TotalSeconds);
                webView21.Reload();
            }
        }

        static async System.Threading.Tasks.Task DelayAsyncSec(double seconds)
        {
            await DelayAsync((int)(seconds * 1000));
        }

        static async System.Threading.Tasks.Task DelayAsync(int milliseconds)
        {
            await System.Threading.Tasks.Task.Delay(milliseconds);
        }

        TimeSpan ParseDurationString(string durationString)
        {
            int hours = 0, minutes = 0, seconds = 0;

            char[] delimiterChars = {'H', 'M', 'S' };

            
            string[] sections = durationString.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            int totalLength = 0;

            for (int i = 0; i < sections.Length - 1; i++)
            {
                sections[i] += durationString[sections[i].Length + totalLength];
                totalLength += sections[i].Length;
            }

            for(int j = 0; j < sections.Length; j++)
            {
                char charToRemove = '"';
                sections[j] = sections[j].Replace(charToRemove.ToString(), "");
            }

            foreach (String str in sections)
            {
                if (str.Contains("H"))
                {
                    hours = int.Parse(str.Replace("H", ""));
                } else if (str.Contains("M"))
                {
                    minutes = int.Parse(str.Replace("M", ""));
                }
                else if (str.Contains("S"))
                {
                    seconds =int.Parse(str.Replace("S", ""));
                }
            }

            // Create a TimeSpan object with the parsed values
            return new TimeSpan(hours, minutes, seconds);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Return)
            {
                if (textBox1.Text.Length > 0)
                {
                    List<string> commandParts = textBox1.Text.Split(' ').ToList();
                    string command = commandParts.First();
                    commandParts.Remove(command);
                    string[] args = commandParts.ToArray();

                    textBox1.Text = "";

                    getWebBrowserCommand(command).executeCommand(command, args);
                }
            }
        }

        interface WebBrowserCommand
        {
            void executeCommand(string command, string[] args);
        }

        class cancelQuitCommand : WebBrowserCommand
        {
            public void executeCommand(string command, string[] args)
            {
                if(args.Length > 0)
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

        class goToCommand : WebBrowserCommand
        {
            public void executeCommand(string command, string[] args)
            {
                if(args.Length < 1 )
                {
                    //Didn't provide a URL
                    Form1._instance.printToConsole("Invalid command arguments! Usage: goto <url>");
                } else
                {
                    Uri result = null;
                    if(Uri.TryCreate(args[0], UriKind.Absolute, out result))
                    {
                        Form1._instance.webView21.Source = result;
                    }
                }
            }
        }

        private void webView21_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            printToConsole($"Url: {e.Uri.ToString()}");

            if (e.Uri.ToString().ToLower().Contains("https://confirmedinitalizationpage".ToLower()))
            {
                printToConsole($"Initalization Complete!");
                showInitalizationScreen = false;
                //create file
                File.Create(filePathFolder + "Initalized.Egario");
                e.Cancel = true;
                Uri source;
                if(Uri.TryCreate("https://www.csgoroll.com/en/boxes/world/daily-free", UriKind.Absolute, out source))
                {
                    textBox2.Visible = true;
                    webView21.Source = source;
                }
            }
        }

        private void showInitalizationPage()
        {
            webView21.NavigateToString("<!DOCTYPE html>\r\n<html lang=\"en\">\r\n\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>CSGORoll Daily Collector</title>\r\n    <style>\r\n        body {\r\n            font-family: 'Arial', sans-serif;\r\n            max-width: 800px;\r\n            margin: 0 auto;\r\n            padding: 20px;\r\n            background-color: #f8f8f8;\r\n            color: #333;\r\n        }\r\n\r\n        h1, h2, h3 {\r\n            color: #3498db;\r\n        }\r\n\r\n        a {\r\n            color: #2ecc71;\r\n            text-decoration: none;\r\n        }\r\n\r\n        ol, ul {\r\n            margin-bottom: 20px;\r\n        }\r\n\r\n        code {\r\n            background-color: #ecf0f1;\r\n            padding: 2px 4px;\r\n            border-radius: 4px;\r\n        }\r\n\r\n        p {\r\n            line-height: 1.6;\r\n        }\r\n\r\n        strong {\r\n            color: #e74c3c;\r\n        }\r\n\r\n        .fixed-button {\r\n            position: fixed;\r\n            bottom: 25px;\r\n            left: 50%;\r\n            transform: translateX(-50%);\r\n            background-color: #2ecc71;\r\n            color: #fff;\r\n            padding: 10px 20px;\r\n            border: none;\r\n            border-radius: 5px;\r\n            cursor: pointer;\r\n            opacity: 1; /* Initially set opacity to make it appear disabled */\r\n            pointer-events: auto; /* Initially disable pointer events */\r\n            transition: opacity 0.3s ease, background-color 0.3s ease; /* Add a smooth transition */\r\n        }\r\n\r\n    </style>\r\n</head>\r\n\r\n<body>\r\n\r\n    <h1>CSGORoll Daily Collector</h1>\r\n    <p>Developed By: TerminatorIsGod <br> Official Github URL: <a href=\"https://github.com/TerminatorIsGod/CSGORoll-Daily-Collector\">https://github.com/TerminatorIsGod/CSGORoll-Daily-Collector</a></p>\r\n\r\n    <h1>Table of Contents</h1>\r\n    <ol>\r\n        <li>What is this amazing free program?</li>\r\n        <li>What's the catch/Why is it free?</li>\r\n        <li>Does CSGORoll allow this?</li>\r\n        <li>How to Use</li>\r\n        <li>Common Issues</li>\r\n        <li>How It Works</li>\r\n        <li>Commands/Console</li>\r\n        <li>Safety/Other</li>\r\n    </ol>\r\n\r\n    <h2>What is this amazing free program?</h2>\r\n    <p>This program automates the collection and sells your free daily cases on CSGORoll. It does this by using Microsoft's WebView2 library to simulate a web browser. We then run some simple javascript to do stuff such as verify you are signed in, check how much longer is left to claim the cases, and to open the cases themselves. One useful feature to enhance its automation capabilities is having it so if you get signed out of CSGORoll, it will sign you back in as long as you are still signed into Steam. At no point do the developers have access to anything, it works as if you are using your regular web browser.</p>\r\n\r\n    <h2>What's the catch/Why is it free?</h2>\r\n    <p>The 'catch' lies in the automatic setting of your affiliate code. This strategic decision not only enables us to provide the program for free but also comes at no cost to you. By setting your affiliate code, you contribute to supporting the developer to continue making programs just like this one.</p>\r\n\r\n    <h2>Does CSGORoll allow this?</h2>\r\n    <p>While CSGORoll's terms and conditions do not expressly prohibit the use of scripts for automating the collection of free daily cases, it's important to understand that the program operates within the existing framework of the website. We want to emphasize that we assume no responsibility for any actions CSGORoll may take regarding your account. Users are encouraged to use the program responsibly and adhere to the terms and conditions of CSGORoll.</p>\r\n\r\n    <h2>How to Use</h2>\r\n    <ol>\r\n        <li>Run the program (congratulations on reaching this step!)</li>\r\n        <li>Read this page</li>\r\n        <li>Press 'Confirm' at the bottom of the page.</li>\r\n        <li>Sign into CSGORoll using Steam</li>\r\n        <li>Watch your daily cases get claimed</li>\r\n    </ol>\r\n    <p><strong>Note:</strong> Ensure your computer is not turned off during the daily case collection time. Sleep mode is acceptable. If your computer is turned off, the program won't run, preventing the collection of your free daily cases.</p>\r\n\r\n    <h2>Common Issues</h2>\r\n    <h3>My daily cases are no longer being collected!</h3>\r\n    <p>If you have moved the exe file then you need to rerun the program.</p>\r\n\r\n    <h3>The program isn't automatically launching itself when im not at my computer!</h3>\r\n    <p>Your computer must be either on or in sleep mode. If your computer is off then the program won't work. Make sure you have also signed in at least once since turning the PC on.</p>\r\n\r\n    <h3>The program isn't automatically closing itself!</h3>\r\n    <p>If the program isn't closing itself after 3 minutes then check to see if you are signed into CSGORoll. If you are signed in then try swapping pages and then going back to the rewards page.</p>\r\n\r\n    <h3>My internet is really, really slow so the program doesn't work.</h3>\r\n    <p>Download the 'slow internet' version of the program.</p>\r\n\r\n    <h2>How It Works</h2>\r\n    <p>This program leverages Microsoft's WebView2 library to simulate a Microsoft Edge browser. By injecting JavaScript, it automates the process of opening daily cases on CSGORoll. Coupled with Windows Task Scheduler, it runs daily even during sleep mode.</p>\r\n\r\n    <h2>Commands/Console</h2>\r\n    <p>There's little need for manual intervention as everything is automated. Using commands unnecessarily may lead to issues. We take no responsibility or provide support in such cases.</p>\r\n\r\n    <h3>Available Commands:</h3>\r\n    <ul>\r\n        <li><code>goTo &lt;url&gt;:</code> Navigates to the specified URL.</li>\r\n        <li><code>autoQuit [bool]:</code> Toggles automatic program closure.</li>\r\n    </ul>\r\n\r\n    <h2>Safety/Other</h2>\r\n\r\n    <p>Do not trust any downloads to this software that isn't from the official github link.</p>\r\n\r\n    <p>This program does not require any elevated privileges and shouldn't trigger anti-virus programs.</p>\r\n\r\n    <p>Anything that the program does automatically is safe and verified but it's always better to be safe than sorry and not trust some random free program. The correct URL should always be shown right above the browser but to be safe and verify that it is correct you can always, right-click on a page and select 'inspect' to view the website's URL.</p>\r\n\r\n    <p>The program does not directly save or send any user data. Microsoft manages any data storage or transmission.</p>\r\n\r\n    <br><br><br>\r\n\r\n\r\n    <a href=\"https://confirmedInitalizationPage\"><button id=\"acceptButton\" class=\"fixed-button\">Continue</button></a>\r\n\r\n</body>\r\n\r\n</html>");
        }
    }
}