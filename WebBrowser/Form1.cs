﻿using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        bool alreadyShowingScreen = false;

        bool quittingApplication = false;

        bool waitingForTime = false;
        string timeLeftStr = "";

        private System.Windows.Forms.Timer timer;
        private int secondsElapsed;

        bool waitingForJavaScriptToComplete = false;

        string filePathFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CSGORollDailyCollector");

        //private bool firstTimeLaunched = false;

        Dictionary<string, WebBrowserCommand> webBrowserCommands = new Dictionary<string, WebBrowserCommand>();

        private const string ProxyConfigFileName = "proxyconfig.txt";

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE (0x08000000) ensures the window does not activate when created
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }

        string webhookURL = "";

        public Form1()
        {
            InitializeComponent();

            _instance = this;

            this.FormClosing += Form1_FormClosing;

            var proxyServer = GetProxyConfigFromFile();

            initalizeAsync(proxyServer);

            taskName = ConfigurationManager.AppSettings["taskName"];

            webhookURL = ConfigurationManager.AppSettings["discordWebhookURL"];

            //Register Commands
            insertWebBrowserCommand("autoQuit", new cancelQuitCommand());
            insertWebBrowserCommand("goTo", new goToCommand());
            insertWebBrowserCommand("clearLogsFile", new clearLogFileCommand());

            //Form1._instance.disableAutoQuit = true;



            AutoCompleteStringCollection autoCompleteCollection = new AutoCompleteStringCollection();
            autoCompleteCollection.AddRange(webBrowserCommands.Keys.ToArray());
            textBox1.AutoCompleteCustomSource = autoCompleteCollection;

            if (!File.Exists(filePathFolder + "Initalized.Egario"))
            {
                //showInitalizationScreen = true; 
                textBox2.Visible = true;
                File.Create(filePathFolder + "Initalized.Egario");
            } else
            {
                textBox2.Visible = true;
            }
        }

        private (string Proxy, string ProxyType, string Username, string Password) GetProxyConfigFromFile()
        {
            string proxy = null;
            string proxyType = null;
            string username = null;
            string password = null;

            // Check if the proxy configuration file exists
            if (File.Exists(ProxyConfigFileName))
            {
                try
                {
                    // Read all lines from the file
                    foreach (var line in File.ReadAllLines(ProxyConfigFileName))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            switch (parts[0].Trim().ToLower())
                            {
                                case "type":
                                    proxyType = parts[1].Trim().ToLower();
                                    break;
                                case "address":
                                    proxy = parts[1].Trim();
                                    break;
                                case "username":
                                    username = parts[1].Trim();
                                    break;
                                case "password":
                                    password = parts[1].Trim();
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    printToConsole("Failed to read the proxy configuration file");
                }
            }

            return (proxy, proxyType, username, password); // Return the proxy details
        }

        bool providedCreds = false;

        private async void initalizeAsync((string Proxy, string ProxyType, string Username, string Password) proxyConfig)
        {
            printToConsole($"Proxy settings: {proxyConfig.Proxy}, Type: {proxyConfig.ProxyType}, Username: {proxyConfig.Username}");

            CoreWebView2EnvironmentOptions options = null;
            if (!string.IsNullOrEmpty(proxyConfig.Proxy))
            {
                string proxyServer = proxyConfig.Proxy;

                if (proxyConfig.ProxyType == "http" || proxyConfig.ProxyType == "https")
                {
                    options = new CoreWebView2EnvironmentOptions($"--proxy-server={proxyServer}");
                    printToConsole($"Using proxy server: {proxyServer}");
                }
                else
                {
                    printToConsole("Unsupported proxy type.");
                }
            }
            else
            {
                printToConsole("No proxy configured.");
            }

            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);

            await webView21.EnsureCoreWebView2Async(environment);
            webView21.CoreWebView2.WebMessageReceived += messagedReceived;

            // Set up proxy authentication if credentials are provided
            if (!string.IsNullOrEmpty(proxyConfig.Username) && !string.IsNullOrEmpty(proxyConfig.Password))
            {
                webView21.CoreWebView2.BasicAuthenticationRequested += (sender, args) =>
                {
                    var challenge = args.Challenge;

                    // Check if the challenge is for the proxy and if credentials are provided
                    if (!providedCreds && !string.IsNullOrEmpty(proxyConfig.Username) && !string.IsNullOrEmpty(proxyConfig.Password))
                    {
                        args.Response.UserName = proxyConfig.Username;
                        args.Response.Password = proxyConfig.Password;

                        providedCreds = true;

                        // Log the authentication process
                        printToConsole("Provided proxy authentication credentials.");
                    }
                    else
                    {
                        // Optionally, cancel the authentication request if no credentials are provided
                        //args.Cancel = true;
                        printToConsole("Username and/or password is most likely incorrect.");
                    }
                };
            }

            webView21.CoreWebView2.Navigate("https://www.csgoroll.com/en/boxes/world/daily-free");

            //("https://www.google.com");

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;

            timer.Start();
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
                } else
                {
                    timer.Stop();
                }
            }
        }

        private void logConsoleText(string text)
        {
            DateTime currentTime = DateTime.Now;

            if (!showInitalizationScreen)
                File.AppendAllText(filePathFolder + "Initalized.Egario", $"[{currentTime}]: {text} \n");
        }

        public void clearConsoleTextFile()
        {
            if (!showInitalizationScreen)
                File.WriteAllText(filePathFolder + "Initalized.Egario", "");
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
            logConsoleText(text);
        }

        public void setURLTextbox(String url)
        {
            richTextBox2.Text = "URL: " + url;
        }

        private int timesReloaded = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            if(secondsElapsed % 5 == 0)
            {
                printToConsole($"Seconds elapsed: {secondsElapsed}");
            }
            if(secondsElapsed >= 15 && !(webView21.Source.ToString().Contains("steamcommunity.com")))
            {
                timesReloaded++;

                if(timesReloaded >= 100)
                {
                    //restart program 

                    Application.Restart();
                    Environment.Exit(0);
                }

                webView21.Reload();
                
                secondsElapsed = 0;
            }
        }

        private async Task<TimeSpan> getTimeLeft()
        {
            try
            {
                waitingForTime = true;
                string script = "async function mainLoop() {\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(100);\r\n    return await getTime();\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nfunction parseTime(timeString) {\r\n    const regex = /(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+)S)?/;\r\n    const match = timeString.match(regex);\r\n\r\n    if (!match) return 0; // Return 0 if the time string format is invalid\r\n\r\n    const hours = parseInt(match[1]) || 0;\r\n    const minutes = parseInt(match[2]) || 0;\r\n    const seconds = parseInt(match[3]) || 0;\r\n\r\n    return hours * 3600 + minutes * 60 + seconds;\r\n}\r\n\r\nfunction formatTime(seconds) {\r\n    const hours = Math.floor(seconds / 3600);\r\n    const minutes = Math.floor((seconds % 3600) / 60);\r\n    const remainingSeconds = seconds % 60;\r\n\r\n    return `${hours}H${minutes}M${remainingSeconds}S`;\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return areThereCasesVisibleOnPage();\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function areThereCasesVisibleOnPage() {\r\n    var cases = document.querySelector('button[data-test=\"open-case\"]');\r\n    return cases != null;\r\n}\r\n\r\nvar attemptedRetry = false;\r\n\r\nasync function getTime() {\r\n    var section = document.querySelector('section.grid.ng-star-inserted cw-box-grid-item-gaming')?.closest('section.grid.ng-star-inserted');\r\n\r\n    while (!section) {\r\n        await delay(100);\r\n        section = document.querySelector('section.grid.ng-star-inserted cw-box-grid-item-gaming')?.closest('section.grid.ng-star-inserted');\r\n    }\r\n\r\n    var buttons = section.querySelectorAll('.open-btn');\r\n    var enabledButtons = Array.from(buttons).filter(button => !button.disabled);\r\n    if (enabledButtons.length > 0) {\r\n        return formatTime(3);\r\n    }\r\n\r\n    var elements = section.querySelectorAll('cw-countdown span:not(.text-warning)');\r\n\r\n    while (elements.length === 0) {\r\n        await delay(100);\r\n        elements = section.querySelectorAll('cw-countdown span:not(.text-warning)');\r\n    }\r\n\r\n    var timesInSeconds = [];\r\n\r\n    for (const element of elements) {\r\n        if (element.innerText.trim() !== '' && /\\d/.test(element.innerText)) {\r\n            const timeInSeconds = parseTime(element.innerText);\r\n            timesInSeconds.push(timeInSeconds);\r\n            console.log(element.innerText);\r\n        }\r\n    }\r\n\r\n    if (timesInSeconds.length === 0) {\r\n        if (attemptedRetry) {\r\n            return \"null\";\r\n        }\r\n\r\n        console.log(\"Failed to find any countdowns... retrying\");\r\n        attemptedRetry = true;\r\n        return getTime();\r\n    }\r\n\r\n    var smallestTimeInSeconds = Math.min(...timesInSeconds);\r\n    var smallestTimeFormatted = formatTime(smallestTimeInSeconds);\r\n    console.log(smallestTimeFormatted); // Log the smallest time to console\r\n    return smallestTimeFormatted;\r\n}\r\n\r\nasync function waitForMessage() {\r\n    var result = await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"TimeMessage: \" + result);\r\n    console.log(result);\r\n}\r\n\r\nwaitForMessage();\r\n";
                var timeLeft = await webView21.ExecuteScriptAsync(script);

                secondsElapsed = 0;

                while (waitingForTime)
                {
                    await DelayAsync(100);

                    if(secondsElapsed >= 10)
                    {
                        timeLeft = await webView21.ExecuteScriptAsync(script);
                        secondsElapsed = 0;
                    }
                }
                printToConsole("Time left: " + timeLeftStr);
                if(timeLeft == null || timeLeft.Equals("null"))
                {
                    return new TimeSpan(0, 0, 0);
                }
                return ParseDurationString(timeLeftStr);
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

                    // If timespan is greater than 13 hours, schedule an additional trigger for 12 hours
                    /*if (timespan.TotalHours > 13)
                    {
                        TimeSpan twelveHours = new TimeSpan(12, 2, 0);
                        DailyTrigger additionalTrigger = new DailyTrigger();
                        additionalTrigger.DaysInterval = 1;
                        additionalTrigger.StartBoundary = DateTime.Now.Add(twelveHours);
                        taskDef.Triggers.Add(additionalTrigger);
                    }*/

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

            // If timespan is greater than 13 hours, schedule an additional trigger for 12 hours
            if (timeSpanToAdd.TotalHours > 13)
            {
                TimeSpan twelveHours = new TimeSpan(12, 2, 0);
                DailyTrigger additionalTrigger = new DailyTrigger();
                additionalTrigger.DaysInterval = 1;
                additionalTrigger.StartBoundary = DateTime.Now.Add(twelveHours);
                taskDef.Triggers.Add(additionalTrigger);
            }

            taskDef.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location));

            ts.RootFolder.RegisterTaskDefinition(taskName, taskDef);
        }

        private void messagedReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            printToConsole(args.TryGetWebMessageAsString());
            string messageStr = args.TryGetWebMessageAsString();
            if(messageStr.Contains("TimeMessage: "))
            {
                timeLeftStr = messageStr.Replace("TimeMessage: ", "");
                waitingForTime = false;
            } else if(messageStr.Contains("JavaScript Completed"))
            {
                waitingForJavaScriptToComplete = false;
            } else if(messageStr.Contains("Reload Page"))
            {
                webView21.Reload();
            }
        }

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (showInitalizationScreen)
            {
                if (!alreadyShowingScreen)
                {
                    showInitalizationPage();
                    alreadyShowingScreen = true;
                }
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
                //Check if still on correct page
                if (!webView21.Source.ToString().Contains("/daily-free"))
                {
                    return;
                }

                await DelayAsync(500);

                openAllCasesChecker();
                //openAllCases();
                //TimeSpan ts = await getTimeLeft();
                /*printToConsole($"Time Seconds Left: {ts.TotalSeconds}");
                if (ts.TotalSeconds > 0)
                {
                    thereIsATimer(ts);
                } else
                {
                    openAllCases();
                }*/
            } else if (webView21.Source.ToString().Contains("/summary"))
            {
                setAffiliateCode();
            }
            
        }

        private CancellationTokenSource _cancellationTokenSource;

        private async System.Threading.Tasks.Task openAllCasesChecker()
        {
            // Cancel the previous operation if it's running
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();

                try
                {
                    // Wait for the previous task to stop
                    await System.Threading.Tasks.Task.Delay(100, _cancellationTokenSource.Token); // Adjust delay as necessary
                }
                catch (TaskCanceledException)
                {
                    // Expected cancellation
                }
            }

            // Create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Run the openAllCases method with the new cancellation token
                await openAllCases(_cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
            }
        }

        private async System.Threading.Tasks.Task openAllCases(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            printToConsole("openAllCases called");
            //printToConsole("Case check 1");
            waitingForJavaScriptToComplete = true;
            string script = "var fakeSellButton = false;\r\n\r\nasync function mainLoop() {\r\n    // Is the page loaded?\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await areAllCasesOpen();\r\n    return true;\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return areThereCasesVisibleOnPage();\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function areThereCasesVisibleOnPage() {\r\n    var cases = document.querySelector('button[data-test=\"open-case\"]');\r\n    return cases !== null;\r\n}\r\n\r\nasync function isCasePanelShowing() {\r\n    var modal = document.querySelector('cw-box-grid');\r\n    document.getElementsByClassName\r\n    return modal !== null;\r\n}\r\n\r\nasync function isCasePanelOpenButtonActive() {\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if (modal) {\r\n        var buttonInsideModal = modal.querySelector('.open-btn');\r\n        await delay(100);\r\n        return buttonInsideModal && !buttonInsideModal.disabled;\r\n    }\r\n    return false;\r\n}\r\n\r\nasync function openCase() {\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if (modal) {\r\n        await delay(100);\r\n        var buttonInsideModal = modal.querySelector('.open-btn');\r\n        await delay(100);\r\n        if (buttonInsideModal) {\r\n            buttonInsideModal.click();\r\n\r\n            await delay(500);\r\n            var buttonInsideModal2 = modal.querySelector('.open-btn');\r\n            if (buttonInsideModal2) {\r\n                console.log(\"Error when opening case... retrying\");\r\n                buttonInsideModal2.click();\r\n            }\r\n\r\n            await delay(500);\r\n            var buttonInsideModal3 = modal.querySelector('.open-btn');\r\n            if (buttonInsideModal3) {\r\n                console.log(\"Error when opening case... retrying\");\r\n                buttonInsideModal3.click();\r\n            }\r\n\r\n            return true;\r\n        }\r\n    }\r\n    return false;\r\n}\r\n\r\nasync function sellItem() {\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if (modal) {\r\n        var buttonInsideModal = modal.querySelector('.sell-btn');\r\n        if (buttonInsideModal) {\r\n            await delay(100);\r\n            buttonInsideModal.click();\r\n            await delay(500);\r\n            buttonInsideModal.click();\r\n        }\r\n    }\r\n}\r\n\r\nasync function closeModal() {\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if (modal) {\r\n        var buttonInsideModal = modal.querySelector('.close');\r\n        await delay(100);\r\n        buttonInsideModal.click();\r\n    }\r\n}\r\n\r\nasync function openAllCases(buttons) {\r\n    var enabledButtons = Array.from(buttons).filter(button => !button.disabled);\r\n\r\n    if (enabledButtons.length > 0) {\r\n        for (const button of enabledButtons) {\r\n            // Press button\r\n            button.click();\r\n            // Await for menu to show\r\n            var isCasePanelShowingWhileLoop = await isCasePanelShowing();\r\n            while (!isCasePanelShowingWhileLoop) {\r\n                console.log(\"Case open panel not active, waiting...\");\r\n                await delay(100);\r\n                isCasePanelShowingWhileLoop = await isCasePanelShowing();\r\n            }\r\n\r\n            await delay(100);\r\n\r\n            var isPanelOpenButtonActiveWhileLoop = await isCasePanelOpenButtonActive();\r\n            while (!isPanelOpenButtonActiveWhileLoop) {\r\n                console.log(\"Button not active, waiting...\");\r\n                await delay(100);\r\n                isPanelOpenButtonActiveWhileLoop = await isCasePanelOpenButtonActive();\r\n            }\r\n\r\n            await delay(100);\r\n\r\n            // Uncomment to open case\r\n            var didItOpenCase = false;\r\n            while (!didItOpenCase) {\r\n                didItOpenCase = await openCase();\r\n            }\r\n\r\n            // Await for sell button\r\n            var isSellButtonShowingWhileLoop = await isSellButtonShowing();\r\n            while (!isSellButtonShowingWhileLoop) {\r\n                await delay(100);\r\n                console.log(\"Sell button not active, waiting...\");\r\n                isSellButtonShowingWhileLoop = await isSellButtonShowing();\r\n\r\n                if (fakeSellButton) {\r\n                    fakeSellButton = false;\r\n                    break;\r\n                }\r\n            }\r\n\r\n            await delay(500);\r\n\r\n            // Press twice\r\n            await sellItem();\r\n            console.log(\"Item sold\");\r\n\r\n            await delay(100);\r\n\r\n            // Close\r\n            await closeModal();\r\n            console.log(\"Closing modal\");\r\n\r\n            // Await for menu to be gone\r\n            var isCasePanelShowingWhileLoopForClosing = await isCasePanelShowing();\r\n            while (isCasePanelShowingWhileLoopForClosing) {\r\n                console.log(\"Modal still open...\");\r\n                await closeModal();\r\n                await delay(100);\r\n                isCasePanelShowingWhileLoopForClosing = await isCasePanelShowing();\r\n            }\r\n\r\n            await delay(100);\r\n\r\n            //await sendReloadPageMessage();\r\n        }\r\n    }\r\n\r\n    return true;\r\n}\r\n\r\nasync function isSellButtonShowing() {\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    var sellButton = modal.querySelector('.sell-btn');\r\n\r\n    if (sellButton && !sellButton.disabled) {\r\n        console.log(\"No sell button showing...\");\r\n        return true;\r\n    }\r\n    return false;\r\n}\r\n\r\nasync function areAllCasesOpen() {\r\n    // Check to see if there are any open buttons; if not, return to application\r\n    var section = document.querySelector('section.grid.ng-star-inserted cw-box-grid-item-gaming')?.closest('section.grid.ng-star-inserted');\r\n\r\n    if (section) {\r\n        // Find all buttons within the section\r\n        var buttons = section.querySelectorAll('.open-btn');\r\n\r\n        var casesAreOpen = false;\r\n        while (!casesAreOpen) {\r\n            casesAreOpen = await openAllCases(buttons);\r\n            await delay(100);\r\n        }\r\n\r\n        await delay(3000);\r\n\r\n        buttons = section.querySelectorAll('.open-btn');\r\n\r\n        casesAreOpen = false;\r\n        while (!casesAreOpen) {\r\n            casesAreOpen = await openAllCases(buttons);\r\n            await delay(100);\r\n        }\r\n\r\n        // Double check\r\n        return true;\r\n    } else {\r\n        console.log('Section not found!');\r\n        // idk do something to fix\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function sendCompletionMessage() {\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nasync function sendReloadPageMessage() {\r\n    chrome.webview.postMessage(\"Reload Page\");\r\n}\r\n\r\nsendCompletionMessage();\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
            await webView21.ExecuteScriptAsync(script);
            printToConsole("openAllCases script executed");
            //await setAffiliateCode();
            //printToConsole("Case check 2");
            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if(secondsElapsed > 15)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    printToConsole("Cancelling old openAllCases...");
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            printToConsole("openAllCases script completed 1");

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            TimeSpan tsA1 = await getTimeLeft();

            await DelayAsync(3000);

            //redundency check
            waitingForJavaScriptToComplete = true;
            await webView21.ExecuteScriptAsync(script);

            printToConsole("openAllCases script executed again");

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if (secondsElapsed > 15)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    printToConsole("Cancelling old openAllCases...");
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            printToConsole("openAllCases script completed 2");

            TimeSpan tsA2 = await getTimeLeft();

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await DelayAsync(3000);

            //redundency check
            waitingForJavaScriptToComplete = true;
            await webView21.ExecuteScriptAsync(script);

            printToConsole("openAllCases script executed again");

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if (secondsElapsed > 15)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    printToConsole("Cancelling old openAllCases...");
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            printToConsole("openAllCases script completed 3");

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            printToConsole("All cases are open");
            TimeSpan ts = await getTimeLeft();

            if (cancellationToken.IsCancellationRequested)
            {
                printToConsole("Cancelling old openAllCases...");
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (tsA2 < ts)
            {
                thereIsATimer(tsA2);
            } else
            {
                thereIsATimer(ts);
            }
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
            await webView21.ExecuteScriptAsync("async function mainLoop(){\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while(!hasPageFullyLoadedCheck){\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(500);\r\n\r\n    goToProfilePage();\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded(){\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if(footer){\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return true;\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n        \r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function isAvatarBtnMenuLoaded(){\r\n    var profileContainer = document.querySelector('.profile-container');\r\n\r\n    if(profileContainer){\r\n        return true;\r\n    }\r\n\r\n    return false;\r\n\r\n}\r\n\r\nasync function goToProfilePage() {\r\n    var profileButtonElement = document.querySelector('.avatar-btn');\r\n    profileButtonElement.click();\r\n\r\n    // Assuming loadPageDelayMiliSec is a predefined variable\r\n    var avatarBtnMenuLoadedBool = await isAvatarBtnMenuLoaded();\r\n    while(!avatarBtnMenuLoadedBool){\r\n        await delay(100);\r\n        avatarBtnMenuLoadedBool = await isAvatarBtnMenuLoaded();\r\n    }\r\n    \r\n    //wait for div\r\n    var mainLinksDiv = document.querySelector('.main-links');\r\n\r\n    var spanElement = mainLinksDiv.querySelector('span[data-test=\"username\"]');\r\n    spanElement.click();\r\n}\r\n\r\nasync function sendCompletionMessage(){\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nsendCompletionMessage();");
        }

        private async System.Threading.Tasks.Task sellAllItems()
        {
            await webView21.ExecuteScriptAsync("async function mainLoop() {\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(500);\r\n\r\n    goToProfilePage();\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return true;\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function isInventoryBtnMenuLoaded() {\r\n    var inventoryContainer = document.querySelector('cw-portable-inventory');\r\n\r\n    if (inventoryContainer) {\r\n        return true;\r\n    }\r\n\r\n    return false;\r\n}\r\n\r\nasync function isSelectAllBtnLoaded(){\r\n    var selectAllButton = document.querySelector('button[data-test=\"select-all\"]');\r\n\r\n    if(selectAllButton) {\r\n        selectAllButton.click();\r\n        return true;\r\n    }\r\n\r\n    return false;\r\n}\r\n\r\nasync function goToProfilePage() {\r\n    var inventoryButtonElement = document.querySelector('.portable-inventory-button');\r\n    inventoryButtonElement.click();\r\n\r\n    var inventoryBtnMenuLoadedBool = await isInventoryBtnMenuLoaded();\r\n    while (!inventoryBtnMenuLoadedBool) {\r\n        await delay(100);\r\n        inventoryBtnMenuLoadedBool = await isInventoryBtnMenuLoaded();\r\n    }\r\n\r\n    var selectAllBtnMenuLoadedBool = await isSelectAllBtnLoaded();\r\n    while (!selectAllBtnMenuLoadedBool) {\r\n        await delay(100);\r\n        selectAllBtnMenuLoadedBool = await isSelectAllBtnLoaded();\r\n    }\r\n    \r\n    await delay(200);\r\n\r\n    var sellbutton = document.querySelector('cw-portable-inventory button.mat-flat-button.mat-accent');\r\n    sellbutton.click();\r\n}\r\n\r\nasync function sendCompletionMessage() {\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nsendCompletionMessage();\r\n");
        }

        private async System.Threading.Tasks.Task setAffiliateCode()
        {
            await webView21.ExecuteScriptAsync("async function mainLoop() {\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(500);\r\n\r\n    await setCode();\r\n\r\n    await delay(500);\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return true;\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function setCode() {\r\n    await delay(500);\r\n    var affiliateCodeSection = document.querySelector('cw-promo-code-redeem-form');\r\n\r\n    while (affiliateCodeSection == null) {\r\n        await delay(100);\r\n        affiliateCodeSection = document.querySelector('cw-promo-code-redeem-form');\r\n    }\r\n\r\n    var affiliateCodeTextbox = affiliateCodeSection.querySelector('[formcontrolname=\"code\"]');\r\n    var applyButton = affiliateCodeSection.querySelector('button');\r\n\r\n    affiliateCodeTextbox.value = \"ABLUE\";\r\n\r\n    await delay(100);\r\n\r\n    const inputEvent = new Event('input', { bubbles: true });\r\n\r\n    affiliateCodeTextbox.dispatchEvent(inputEvent);\r\n\r\n    await delay(100);\r\n\r\n    applyButton.dispatchEvent(inputEvent);\r\n\r\n    applyButton.click(); \r\n   \r\n}\r\n\r\nasync function activeCode() {\r\n    var depositButton = document.querySelector('a[data-test=\"purchase-button\"]');\r\n\r\n    var i = 0;\r\n    while (depositButton == null && i < 50) {\r\n        await delay(100);\r\n        i++;\r\n        depositButton = document.querySelector('a[data-test=\"purchase-button\"]');\r\n    }\r\n\r\n    if(depositButton == null){\r\n        return;\r\n    }\r\n\r\n    depositButton.click();\r\n\r\n    await delay(100);\r\n\r\n    var inputCodeElement = document.querySelector('input.mat-input-element.code-input#mat-input-6');\r\n\r\n    var j = 0;\r\n    while (inputCodeElement == null && j < 50) {\r\n        await delay(100);\r\n        j++;\r\n        inputCodeElement = document.getElementById('mat-input-9');\r\n    }\r\n\r\n    if(inputCodeElement == null) {\r\n        console.log(\"Input code element null returning...\");\r\n        return;\r\n    }\r\n\r\n    inputCodeElement.value = \"EGAR.IO\";\r\n\r\n    await delay(100);\r\n\r\n    const inputEvent = new Event('input', { bubbles: true });\r\n    inputCodeElement.dispatchEvent(inputEvent);\r\n\r\n    var buttons = document.querySelectorAll('button');\r\n\r\n    // Iterate over the buttons and find the one with 'Apply' text that is not disabled\r\n    buttons.forEach(function(button) {\r\n        var buttonText = button.querySelector('.mat-button-wrapper')?.textContent.trim();\r\n        var isDisabled = button.hasAttribute('disabled') || button.classList.contains('mat-button-disabled');\r\n        \r\n        if (buttonText === 'Apply' && !isDisabled) {\r\n            console.log(\"button pressed\");\r\n            button.click(); // Click the button\r\n        }\r\n    });\r\n\r\n    console.log(\"Completed activating!\");\r\n\r\n}\r\n\r\nasync function sendCompletionMessage() {\r\n    await mainLoop();\r\n    await delay(100);\r\n    await activeCode();\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nsendCompletionMessage();\r\n");
        }

        private async System.Threading.Tasks.Task sendDiscordWebhookMessage()
        {
            await webView21.ExecuteScriptAsync($"var webhookURL = \"{webhookURL}\";" + "\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nfunction sendMessage(balance, accountName) {\r\n    var request = new XMLHttpRequest();\r\n    request.open(\"POST\", webhookURL);\r\n\r\n    request.setRequestHeader('Content-type', 'application/json');\r\n\r\n    var params = {\r\n        username: \"CSGORoll Drop Notifier\",\r\n        avatar_url: \"https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp\",\r\n        embeds: [\r\n            {\r\n                title: \"Your cases have been claimed!\",\r\n                description: \"Account Name: `\" + accountName + \"` \\nBalance: `\" + balance +\"` \\n\\n\",\r\n                color: 0xfb2b23, \r\n                timestamp: new Date(),\r\n                footer: {\r\n                    text: \"https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot\"\r\n                },\r\n                thumbnail: {\r\n                    url: \"https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp\"\r\n                },\r\n                author: {\r\n                    name: \"CSGORoll Daily Cases Collector\",\r\n                    url: \"https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot\",\r\n                    icon_url: \"https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp\"\r\n                }\r\n            }\r\n        ]\r\n    };\r\n\r\n    request.send(JSON.stringify(params));\r\n}\r\n\r\nasync function isAvatarBtnMenuLoaded() {\r\n    var profileContainer = document.querySelector('.profile-container');\r\n    return !!profileContainer;\r\n}\r\n\r\nasync function sendMessageToDiscord(){\r\n    var profileButtonElement = document.querySelector('.avatar-btn');\r\n    profileButtonElement.click();\r\n\r\n    // Wait until the profile menu is loaded\r\n    var avatarBtnMenuLoadedBool = await isAvatarBtnMenuLoaded();\r\n    while (!avatarBtnMenuLoadedBool) {\r\n        await delay(100);\r\n        avatarBtnMenuLoadedBool = await isAvatarBtnMenuLoaded();\r\n    }\r\n\r\n    var mainLinksDiv = document.querySelector('.main-links');\r\n    var spanElement = mainLinksDiv.querySelector('span[data-test=\"username\"]');\r\n\r\n    var username = \"USERNAME\";\r\n\r\n    if (spanElement) {\r\n        username = spanElement.textContent.trim();\r\n        console.log(\"Username:\", username); // Log the text content of the span\r\n    } else {\r\n        console.error(\"Username element not found.\");\r\n    }\r\n\r\n    var balanceElement = document.querySelector('span[data-test=\"value\"]');\r\n    var balance = \"BALANCE\";\r\n\r\n    // Check if the element exists and get its text content\r\n    if (balanceElement) {\r\n        balance = balanceElement.textContent.trim();\r\n        console.log(\"Balance:\", balance); // Logs \"2.72\"\r\n    } else {\r\n        console.error(\"Balance element not found.\");\r\n    }\r\n\r\n    sendMessage(balance, username);\r\n}\r\n\r\nfunction checkIfRegularThirtyMinDropReady() {\r\n    var currentTime = new Date();\r\n    var minutes = currentTime.getMinutes();\r\n\r\n    if (minutes === 59 || minutes === 29) {\r\n        console.log(\"The time is an interval of 30 minutes.\");\r\n        if (!triggeredThirtyMinDropMessage) {\r\n            console.log(\"Sending notification!\");\r\n            sendMessage(\"Regular 30-minute drop is about to begin!\");\r\n            triggeredThirtyMinDropMessage = true;\r\n        } else {\r\n            console.log(\"Already sent notification\");\r\n        }\r\n    } else {\r\n        console.log(\"The time is not an interval of 30 minutes.\");\r\n    }\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return true;\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function mainLoop() {\r\n    // Is the page loaded?\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await sendMessageToDiscord();\r\n    return true;\r\n}\r\n\r\nasync function sendCompletionMessage() {\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nasync function sendReloadPageMessage() {\r\n    chrome.webview.postMessage(\"Reload Page\");\r\n}\r\n\r\nsendCompletionMessage();\r\n");
        }

        private async void thereIsATimer(TimeSpan ts)
        {
            if(ts.TotalSeconds >= 300)
            {
                //Reschedule
                printToConsole("Timer is greater than 300 seconds, rescheduling...");
                updateTaskSchedularTask(ts);

                waitingForJavaScriptToComplete = true;
                await sellAllItems();
                secondsElapsed = 0;
                while (waitingForJavaScriptToComplete)
                {
                    await DelayAsync(100);
                    if (secondsElapsed > 15)
                    {
                        secondsElapsed = 0;
                        waitingForJavaScriptToComplete = false;
                        webView21.Refresh();
                        await DelayAsync(100);
                    }
                }

                waitingForJavaScriptToComplete = true;
                await goToProfilePage();
                secondsElapsed = 0;
                while (waitingForJavaScriptToComplete)
                {
                    await DelayAsync(100);
                    if(secondsElapsed > 15)
                    {
                        secondsElapsed = 0;
                        await goToProfilePage();
                    }
                }

                await DelayAsync(500);

                if(webhookURL != "")
                {
                    printToConsole("Sending discord webhook message...");
                    waitingForJavaScriptToComplete = true;
                    await sendDiscordWebhookMessage();//do if to check if webhook was set
                    secondsElapsed = 0;
                    while (waitingForJavaScriptToComplete)
                    {
                        await DelayAsync(100);
                        if (secondsElapsed > 20)
                        {
                            secondsElapsed = 0;
                            await sendDiscordWebhookMessage();
                        }
                    }
                    printToConsole("Sent discord webhook message...");
                } else
                {
                    printToConsole("No webhook URL found, not sending message...");
                }
                

                waitingForJavaScriptToComplete = true;
                await setAffiliateCode();
                secondsElapsed = 0;
                while (waitingForJavaScriptToComplete)
                {
                    await DelayAsync(100);
                    if (secondsElapsed > 15)
                    {
                        secondsElapsed = 0;
                        await setAffiliateCode();
                    }
                }

                await DelayAsync(500);

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

        class clearLogFileCommand : WebBrowserCommand
        {
            public void executeCommand(string command, string[] args)
            {
                Form1._instance.clearConsoleTextFile();
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

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}