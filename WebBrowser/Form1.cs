using Microsoft.Web.WebView2.Core;
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

        private Timer timer;
        private int secondsElapsed;

        bool waitingForJavaScriptToComplete = false;

        string filePathFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CSGORollDailyCollector");

        //private bool firstTimeLaunched = false;

        Dictionary<string, WebBrowserCommand> webBrowserCommands = new Dictionary<string, WebBrowserCommand>();

        public Form1()
        {
            InitializeComponent();

            _instance = this;

            this.FormClosing += Form1_FormClosing;

            initalizeAsync();

            taskName = ConfigurationManager.AppSettings["taskName"];

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
                showInitalizationScreen = true; 
            } else
            {
                textBox2.Visible = true;
            }
        }

        private async void initalizeAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.WebMessageReceived += messagedReceived;

            timer = new Timer();
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            if(secondsElapsed % 5 == 0)
            {
                printToConsole($"Seconds elapsed: {secondsElapsed}");
            }
            if(secondsElapsed >= 120)
            {
                webView21.Reload();
                secondsElapsed = 0;
            }
        }

        private async Task<TimeSpan> getTimeLeft()
        {
            try
            {
                waitingForTime = true;
                string script = "async function mainLoop() {\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while (!hasPageFullyLoadedCheck) {\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(100);\r\n    return await getTime();\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nfunction parseTime(timeString) {\r\n    const regex = /(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+)S)?/;\r\n    const match = timeString.match(regex);\r\n\r\n    if (!match) return 0; // Return 0 if the time string format is invalid\r\n\r\n    const hours = parseInt(match[1]) || 0;\r\n    const minutes = parseInt(match[2]) || 0;\r\n    const seconds = parseInt(match[3]) || 0;\r\n\r\n    return hours * 3600 + minutes * 60 + seconds;\r\n}\r\n\r\nfunction formatTime(seconds) {\r\n    const hours = Math.floor(seconds / 3600);\r\n    const minutes = Math.floor((seconds % 3600) / 60);\r\n    const remainingSeconds = seconds % 60;\r\n\r\n    return `${hours}H${minutes}M${remainingSeconds}S`;\r\n}\r\n\r\nasync function hasPageFullyLoaded() {\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if (footer) {\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return areThereCasesVisibleOnPage();\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function areThereCasesVisibleOnPage() {\r\n    var cases = document.querySelector('button[data-test=\"open-case\"]');\r\n\r\n    if (cases == null) {\r\n        return false;\r\n    }\r\n\r\n    return true;\r\n}\r\n\r\nvar attemptedRetry = false;\r\n\r\nasync function getTime() {\r\n    var section = document.querySelector('section.grid.gaming');\r\n\r\n    while (!section) {\r\n        await delay(100);\r\n        section = document.querySelector('section.grid.gaming');\r\n    }\r\n\r\n    var buttons = section.querySelectorAll('.open-btn');\r\n    var enabledButtons = Array.from(buttons).filter(button => !button.disabled);\r\n    if (enabledButtons.length > 0) {\r\n        return formatTime(3);\r\n    }\r\n\r\n    var elements = section.querySelectorAll('cw-countdown span:not(.text-warning)');\r\n\r\n    while (elements.length == 0) {\r\n        await delay(100);\r\n        elements = section.querySelectorAll('cw-countdown span:not(.text-warning)');\r\n    }\r\n\r\n    var timesInSeconds = [];\r\n\r\n    for (const element of elements) {\r\n        if (element.innerText.trim() !== '' && /\\d/.test(element.innerText)) {\r\n            const timeInSeconds = parseTime(element.innerText);\r\n            timesInSeconds.push(timeInSeconds);\r\n            console.log(element.innerText);\r\n        }\r\n    }\r\n\r\n    if (timesInSeconds.length === 0) {\r\n        if(attemptedRetry){\r\n            return \"null\";\r\n        }\r\n\r\n        console.log(\"Failed to find any countdowns... retrying\");\r\n        attemptedRetry = true;\r\n        return getTime();\r\n    }\r\n\r\n    var smallestTimeInSeconds = Math.min(...timesInSeconds);\r\n    var smallestTimeFormatted = formatTime(smallestTimeInSeconds);\r\n    console.log(smallestTimeFormatted); // Log the smallest time to console\r\n    return smallestTimeFormatted;\r\n}\r\n\r\nasync function waitForMessage() {\r\n    var result = await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"TimeMessage: \" + result);\r\n    console.log(result);\r\n}\r\n\r\nwaitForMessage();";
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

                openAllCases();
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

        private async void openAllCases()
        {
            printToConsole("openAllCases called");
            //printToConsole("Case check 1");
            waitingForJavaScriptToComplete = true;
            string script = "\r\nvar fakeSellButton = false;\r\n\r\nasync function mainLoop(){\r\n    //is page loaded?\r\n    \r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while(!hasPageFullyLoadedCheck){\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n    //\r\n    await areAllCasesOpen();\r\n\r\n    return true;\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded(){\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if(footer){\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return areThereCasesVisibleOnPage();\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n        \r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function areThereCasesVisibleOnPage(){\r\n    var cases = document.querySelector('button[data-test=\"open-case\"]');\r\n\r\n    if(cases == null){\r\n        return false;\r\n    }\r\n\r\n    return true;\r\n}\r\n\r\nasync function isCasePanelShowing(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if(modal){\r\n        return true;\r\n    }\r\n\r\n    return false;\r\n}\r\n\r\nasync function isCasePanelOpenButtonActive(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if(modal){\r\n        var buttonInsideModal = modal.querySelector('.open-btn');\r\n        await delay(100);\r\n        if(buttonInsideModal && !buttonInsideModal.disabled){\r\n            return true;\r\n        }\r\n    }\r\n    return false;\r\n}\r\n\r\nasync function openCase(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if(modal){\r\n        await delay(100);\r\n        var buttonInsideModal = modal.querySelector('.open-btn');\r\n        await delay(100);\r\n        if(buttonInsideModal){\r\n            buttonInsideModal.click();\r\n            return true;\r\n        }\r\n    }\r\n\r\n    return false;\r\n}\r\n\r\nasync function sellItem(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if(modal){\r\n        var buttonInsideModal = modal.querySelector('.sell-btn');\r\n\r\n        if(buttonInsideModal){\r\n            await delay(100);\r\n            buttonInsideModal.click();\r\n            await delay(500);\r\n            buttonInsideModal.click();\r\n        }\r\n    }\r\n}\r\n\r\nasync function closeModal(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    if(modal){\r\n        var buttonInsideModal = modal.querySelector('.close');\r\n        await delay(100);\r\n        buttonInsideModal.click();\r\n    }\r\n}\r\n\r\nasync function openAllCases(buttons){\r\n    var enabledButtons = Array.from(buttons).filter(button => !button.disabled);\r\n\r\n    if(enabledButtons.length > 0){\r\n        for(const button of enabledButtons){\r\n            //press button\r\n            button.click();\r\n            //await for menu to show\r\n            var isCasePanelShowingWhileLoop = await isCasePanelShowing();\r\n            while(!isCasePanelShowingWhileLoop){\r\n                console.log(\"Case open panel not active, waiting...\");\r\n                await delay(100);\r\n                isCasePanelShowingWhileLoop = await isCasePanelShowing();\r\n            }\r\n\r\n            await delay(100);\r\n\r\n            var isPanelOpenButtonActiveWhileLoop = await isCasePanelOpenButtonActive();\r\n            while(!isPanelOpenButtonActiveWhileLoop){\r\n                console.log(\"Button not active, waiting...\");\r\n                await delay(100);\r\n                isPanelOpenButtonActiveWhileLoop = await isCasePanelOpenButtonActive();\r\n            }\r\n\r\n            await delay(100);\r\n\r\n            //Uncomment to open case\r\n            var didItOpenCase = false;\r\n            while(!didItOpenCase){\r\n                didItOpenCase = await openCase();\r\n            }\r\n\r\n            //await for sell button\r\n            var isSellButtonShowingWhileLoop = await isSellButtonShowing();\r\n            while(!isSellButtonShowingWhileLoop){\r\n                await delay(100);\r\n                console.log(\"Sell button not active, waiting...\");\r\n                isSellButtonShowingWhileLoop = await isSellButtonShowing();\r\n\r\n                if(fakeSellButton){\r\n                    fakeSellButton = false;\r\n                    break;\r\n                }\r\n            }\r\n\r\n            await delay(500);\r\n\r\n            //press twice\r\n            await sellItem();\r\n            console.log(\"Item sold\");\r\n\r\n            await delay(100);\r\n\r\n            //close\r\n            await closeModal();\r\n            console.log(\"Closing modal\");\r\n\r\n            //await for menu to be gone\r\n            var isCasePanelShowingWhileLoopForClosing = await isCasePanelShowing();\r\n            while(isCasePanelShowingWhileLoopForClosing){\r\n                console.log(\"Modal still open...\");\r\n                await closeModal();\r\n                await delay(100);\r\n                isCasePanelShowingWhileLoopForClosing = await isCasePanelShowing();\r\n            }\r\n\r\n            await delay(100);\r\n        }\r\n    }\r\n\r\n    return true;\r\n}\r\n\r\nasync function isSellButtonShowing(){\r\n    var modal = document.querySelector('cw-quick-unbox-modal');\r\n    var sellButton = modal.querySelector('.sell-btn');\r\n\r\n    if(sellButton && !sellButton.disabled){\r\n        console.log(\"No sell button showing...\");\r\n        return true;\r\n    }\r\n    return false;\r\n}\r\n\r\nasync function areAllCasesOpen(){\r\n    //check to see if there are any open btns if there aren't then return to application\r\n    var section = document.querySelector('section.grid.gaming');\r\n\r\n    if (section) {\r\n        // Find all buttons within the section\r\n        var buttons = section.querySelectorAll('.open-btn');\r\n        \r\n\r\n        var casesAreOpen = false;\r\n        while(!casesAreOpen){\r\n            casesAreOpen = await openAllCases(buttons);\r\n            await delay(100);\r\n        }\r\n\r\n        await delay(3000);\r\n\r\n        buttons = section.querySelectorAll('.open-btn');\r\n\r\n        casesAreOpen = false;\r\n        while(!casesAreOpen){\r\n            casesAreOpen = await openAllCases(buttons);\r\n            await delay(100);\r\n        }\r\n\r\n        //double check\r\n\r\n        return true;\r\n    \r\n        // Loop through the buttons and do something with them\r\n        \r\n\r\n    } else {\r\n        console.log('Section not found!');\r\n        //idk do something to fix\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function sendCompletionMessage(){\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nsendCompletionMessage();";
            await webView21.ExecuteScriptAsync(script);
            printToConsole("openAllCases script executed");
            //await setAffiliateCode();
            //printToConsole("Case check 2");
            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if(secondsElapsed > 120)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
            }

            printToConsole("openAllCases script completed 1");

            TimeSpan tsA1 = await getTimeLeft();

            await DelayAsync(3000);

            //redundency check
            waitingForJavaScriptToComplete = true;
            await webView21.ExecuteScriptAsync(script);

            printToConsole("openAllCases script executed again");

            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if (secondsElapsed > 120)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
            }

            printToConsole("openAllCases script completed 2");

            TimeSpan tsA2 = await getTimeLeft();

            await DelayAsync(3000);

            //redundency check
            waitingForJavaScriptToComplete = true;
            await webView21.ExecuteScriptAsync(script);

            printToConsole("openAllCases script executed again");

            secondsElapsed = 0;
            while (waitingForJavaScriptToComplete)
            {
                await DelayAsync(100);
                if (secondsElapsed > 120)
                {
                    secondsElapsed = 0;
                    await webView21.ExecuteScriptAsync(script);
                }
            }

            printToConsole("openAllCases script completed 3");

            printToConsole("All cases are open");
            TimeSpan ts = await getTimeLeft();

            if(tsA2 < ts)
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

        private async System.Threading.Tasks.Task setAffiliateCode()
        {
            await webView21.ExecuteScriptAsync("async function mainLoop(){\r\n    var hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    while(!hasPageFullyLoadedCheck){\r\n        console.log(\"Page hasn't loaded yet...\");\r\n        await delay(100);\r\n        hasPageFullyLoadedCheck = await hasPageFullyLoaded();\r\n    }\r\n\r\n    await delay(500);\r\n\r\n    await setCode();\r\n\r\n    await delay(500);\r\n}\r\n\r\nasync function delay(time) {\r\n    return new Promise(resolve => setTimeout(resolve, time));\r\n}\r\n\r\nasync function hasPageFullyLoaded(){\r\n    var loader = document.querySelector('cw-loader.absolute');\r\n    var footer = document.querySelector('cw-footer');\r\n\r\n    if (document.readyState === 'complete') {\r\n        if(footer){\r\n            if (!loader) {\r\n                return true;\r\n            } else {\r\n                var spinnerContainer = loader.querySelector('.spinner-container');\r\n                if (spinnerContainer) {\r\n                    return false;\r\n                } else {\r\n                    return true;\r\n                }\r\n            }\r\n        } else {\r\n            return false;\r\n        }\r\n        \r\n    } else {\r\n        return false;\r\n    }\r\n}\r\n\r\nasync function setCode() {\r\n    await delay(500);\r\n    var affiliateCodeSection = document.querySelector('cw-promo-code-redeem-form');\r\n\r\n    while(affiliateCodeSection == null){\r\n        await delay(100);\r\n        document.querySelector('cw-promo-code-redeem-form');\r\n    }\r\n\r\n    var affiliateCodeTextbox = affiliateCodeSection.querySelector('[formcontrolname=\"code\"]');\r\n    var applyButton = affiliateCodeSection.querySelector('button');\r\n\r\n    affiliateCodeTextbox.value = \"EGAR.IO\";\r\n    \r\n    await delay(100);\r\n\r\n    const inputEvent = new Event('input', { bubbles: true });\r\n\r\n    affiliateCodeTextbox.dispatchEvent(inputEvent);\r\n\r\n    await delay(100);\r\n\r\n    applyButton.dispatchEvent(inputEvent);\r\n\r\n    applyButton.click();\r\n}\r\n\r\nasync function sendCompletionMessage(){\r\n    await mainLoop();\r\n    await delay(100);\r\n    chrome.webview.postMessage(\"JavaScript Completed\");\r\n}\r\n\r\nsendCompletionMessage();");
        }

        private async void thereIsATimer(TimeSpan ts)
        {
            if(ts.TotalSeconds >= 300)
            {
                //Reschedule
                printToConsole("Timer is greater than 300 seconds, rescheduling...");
                updateTaskSchedularTask(ts);

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
    }
}