using Microsoft.Web.WebView2.Core;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebBrowser.commands;
using WebBrowser.WebBrowserCommands.commands;
using WebBrowser.Config;
using WebBrowser.WebBrowserJavaScriptInjections.deserialization;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.steam;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.actions;
using System.Text;
using System.Security.Cryptography;
using System.Security.Principal;
using WebBrowser.Networking;
using System.Drawing;


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

        //string filePathFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CSGORollDailyCollector");

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

        public Form1()
        {
            InitializeComponent();

            _instance = this;

            this.FormClosing += Form1_FormClosing;

            //File for logging
            if (!File.Exists(ConfigManager._Instance.GetLogsFileLocation()))
            {
                FileStream fs = File.Create(ConfigManager._Instance.GetLogsFileLocation());
                fs.Close();
            }

            var proxyServer = GetProxyConfigFromFile();

            initalizeAsync(proxyServer);

            taskName = ConfigManager._Instance.GetConfigFile().taskSchedulerTaskName;

            //setup CommManager
            new CommManager();

            //Register Commands
            insertWebBrowserCommand("autoQuit", new cancelQuitCommand());
            insertWebBrowserCommand("goTo", new goToCommand());
            insertWebBrowserCommand("clearLogsFile", new clearLogFileCommand());

            //printToConsole(new openCaseFull().GetJavaScript(new Dictionary<string, string>{ { "boxId", "boxIDGoesHere" } } ));

            //Form1._instance.disableAutoQuit = true;

            //FindProxies();


            AutoCompleteStringCollection autoCompleteCollection = new AutoCompleteStringCollection();
            autoCompleteCollection.AddRange(webBrowserCommands.Keys.ToArray());
            textBox1.AutoCompleteCustomSource = autoCompleteCollection;

            textBox2.Visible = true;
        }

        public Microsoft.Web.WebView2.WinForms.WebView2 GetWebviewObj()
        {
            return webView21;
        }

        private (string Proxy, string ProxyType, string Username, string Password) GetProxyConfigFromFile()
        {
            string proxy = null;
            string proxyType = null;
            string username = null;
            string password = null;

            // Check if the proxy configuration file exists
            if (ConfigManager._Instance.GetConfigFile().proxyAddress != "")
            {
                proxy = ConfigManager._Instance.GetConfigFile().proxyAddress;
                proxyType = ConfigManager._Instance.GetConfigFile().proxyType;
                username = ConfigManager._Instance.GetConfigFile().proxyUsername;
                password = ConfigManager._Instance.GetConfigFile().proxyPassword;
            }

            return (proxy, proxyType, username, password); // Return the proxy details
        }

        private void FindProxies() //(string Proxy, string ProxyType)
        {
            string proxyUrl = "https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=1000&country=US&ssl=all&anonymity=all";

            HttpClient client = null;
            try
            {
                // Step 1: Create HttpClient
                client = new HttpClient();

                // Step 2: Download proxy list synchronously
                string proxyList = client.GetStringAsync(proxyUrl).GetAwaiter().GetResult();

                // Step 3: Process the proxies
                List<string> proxies = proxyList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Step 4: Output the filtered proxies
                Console.WriteLine("Filtered Proxies:");
                foreach (var proxy in proxies)
                {
                    Console.WriteLine(proxy);
                    Console.WriteLine($"Proxy working: {TestProxy(proxy, "https://httpbin.org/ip", TimeSpan.FromSeconds(5))}, Proxy: {proxy}");
                    //return (proxy, "http");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Dispose HttpClient
                if (client != null)
                {
                    client.Dispose();
                }
            }
        }

        bool TestProxy(string proxyAddress, string testUrl, TimeSpan timeout)
        {
            string[] parts = proxyAddress.Split(':');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid proxy format. Use IP:Port.");
                return false;
            }

            string proxyIp = parts[0];
            int proxyPort = int.Parse(parts[1]);

            try
            {
                // Set up the proxy
                var proxy = new WebProxy(proxyIp, proxyPort)
                {
                    BypassProxyOnLocal = false // Ensures all requests go through the proxy
                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                // Create HttpClient with the proxy handler
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    // Set timeout
                    client.Timeout = timeout;

                    // Send a request to the test URL
                    HttpResponseMessage response = client.GetAsync(testUrl).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        // Display the response (IP address should match the proxy's IP)
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        Console.WriteLine($"Response: {responseContent}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Proxy failed with status code: {response.StatusCode}");
                        return false;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Request timed out. The proxy may be slow or not working.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing proxy: {ex.Message}");
                return false;
            }
        }

        bool providedCreds = false;

        bool usinProxy = false;

        private async void initalizeAsync((string Proxy, string ProxyType, string Username, string Password) proxyConfig)
        {
            printToConsole($"CSGORoll Daily Collector Version: {Program.CurrentVersion}");

            printToConsole($"Proxy settings: {proxyConfig.Proxy}, Type: {proxyConfig.ProxyType}, Username: {proxyConfig.Username}");

            CoreWebView2EnvironmentOptions options = null;
            if (!string.IsNullOrEmpty(proxyConfig.Proxy))
            {
                string proxyServer = proxyConfig.Proxy;

                if (proxyConfig.ProxyType == "http" || proxyConfig.ProxyType == "https")
                {
                    options = new CoreWebView2EnvironmentOptions($"--proxy-server={proxyServer}");
                    printToConsole($"Using proxy server: {proxyServer}");
                    usinProxy = true;
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

            webView21.CoreWebView2InitializationCompleted += async (sender, e) =>
            {
                if (e.IsSuccess)
                {
                    if(!ConfigManager._Instance.GetConfigFile().enableLocalBotComm)
                    {
                        printToConsole("Clearing cookies for csgoroll...");
                        // Clear only cookies for csgoroll.com
                        /*var cookieManager = webView21.CoreWebView2.CookieManager;
                        var cookies = await cookieManager.GetCookiesAsync("https://csgoroll.com");

                        foreach (var cookie in cookies)
                        {
                            cookieManager.DeleteCookie(cookie);
                            printToConsole("Deleted a cookie...");
                        }*/

                        var cookieManager = webView21.CoreWebView2.CookieManager;
                        var allCookies = await cookieManager.GetCookiesAsync(null); // Get all cookies

                        foreach (var cookie in allCookies)
                        {
                            if (cookie.Domain.Contains("csgoroll"))
                            {
                                cookieManager.DeleteCookie(cookie);
                                printToConsole($"Deleted cookie: {cookie.Name} from {cookie.Domain}");
                            }
                        }

                        printToConsole("Finished clearing cookies.");

                        printToConsole("Clearing cache...");

                        // also clear cache
                        await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync(
                            CoreWebView2BrowsingDataKinds.DiskCache
                        );

                        printToConsole("Finished clearing cache.");
                    }
                }
            };

            await webView21.EnsureCoreWebView2Async(environment);

            webView21.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            webView21.CoreWebView2.WebMessageReceived += messagedReceived;

            webView21.CoreWebView2.WebResourceRequested += webView_WebResourceRequested;

            // webView21.CoreWebView2.WebResourceResponseReceived += WebView_WebResourceResponseReceived;


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

        private void WebView_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            printToConsole($"Resource response received: {e.Request.Uri}");
        }

        private static List<string> allowedOperations = new List<string>
        {
            "promo",
            "create",
            "box",
            "user",
            "item"
        };

        private void webView_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            //printToConsole($"Resource requested: {e.Request.Uri}");

            return;

            var uri = e.Request.Uri;

            if(!e.Request.Uri.Contains("csgoroll"))
            {
                return;
            }

            /*foreach(string allowedOp in allowedOperations)
            {
                if (uri.ToLower().Contains(allowedOp.ToLower()))
                {
                    printToConsole($"Resource allowed: {uri}");
                    return;
                }
            }*/

            if (uri.Contains("https://api.csgoroll.com/graphql"))
            {
                printToConsole($"Resource allowed: {uri}");
                return;
            }

            var resourceContext = e.ResourceContext;

            e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
            return;

            if (resourceContext == CoreWebView2WebResourceContext.Image)
            {
                // Block image
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked image: {uri}");
            }
            else if (resourceContext == CoreWebView2WebResourceContext.Websocket)
            {
                // Block WebSocket – NOTE: May not work as expected (see below)
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked socket: {uri}");
            }
            else if (resourceContext == CoreWebView2WebResourceContext.Font)
            {
                // Block WebSocket – NOTE: May not work as expected (see below)
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked font: {uri}");
            }
            else if (resourceContext == CoreWebView2WebResourceContext.Stylesheet)
            {
                // Block WebSocket – NOTE: May not work as expected (see below)
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked stylesheet: {uri}");
            }
            else if (resourceContext == CoreWebView2WebResourceContext.Media)
            {
                // Block WebSocket – NOTE: May not work as expected (see below)
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked media: {uri}");
            }
            else if (resourceContext == CoreWebView2WebResourceContext.All)
            {
                // Block WebSocket – NOTE: May not work as expected (see below)
                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", null);
                printToConsole($"Blocked all: {uri}");
            }

            /* foreach (var bop in blockedOperations)
            {
                if (uri.Contains(bop))
                {
                    e.Response = null;
                    printToConsole($"Blocked operation: {uri}");
                    return;
                }
            }

            // Block images (JPG, PNG, GIF, WebP)
            if (uri.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                uri.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                // Respond with an empty image (1-byte)
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);
                sw.Write("");  // Empty content (transparent 1x1 image)
                sw.Flush();
                ms.Position = 0;

                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "");
                e.Response.Headers.AppendHeader("Content-Type", "image/jpeg");
            }
            // Block videos (MP4, WebM, OGG)
            else if (uri.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                     uri.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
                     uri.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                // Respond with an empty video (1-byte)
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);
                sw.Write("");  // Empty content (dummy video file)
                sw.Flush();
                ms.Position = 0;

                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "");
                e.Response.Headers.AppendHeader("Content-Type", "video/mp4");
            }
            // Block WebSocket connections (ws://, wss://)
            else if (uri.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) ||
                     uri.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
            {
                // Block WebSocket request with empty response
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true);
                sw.Write("");  // Empty response
                sw.Flush();
                ms.Position = 0;

                e.Response = webView21.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "");
                e.Response.Headers.AppendHeader("Content-Type", "text/plain");
            } */
        }

        private async void InjectJavaScriptToBlockMedia()
        {
            // Inject JavaScript to block all images and videos in the page
            string script = @"
                document.querySelectorAll('img').forEach(img => img.src = 'about:blank');
                document.querySelectorAll('video').forEach(video => video.src = 'about:blank');
            ";

            await webView21.ExecuteScriptAsync(script);
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

            string[] allLogs = File.ReadAllLines(ConfigManager._Instance.GetLogsFileLocation());
            if (allLogs.Length >= 1000)
            {
                int half = allLogs.Length / 2;
                string[] newLines = allLogs.Skip(half).ToArray();

                File.WriteAllLines(ConfigManager._Instance.GetLogsFileLocation(), newLines);
            }

            try
            {
                File.AppendAllText(ConfigManager._Instance.GetLogsFileLocation(), $"[{currentTime}]: {text} \n");
            } catch (Exception ex)
            {
                Console.WriteLine("File still open");
            }
            
        }

        public void clearConsoleTextFile()
        {
            File.WriteAllText(ConfigManager._Instance.GetLogsFileLocation(), "");
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

        private List<(float time, object tcs)> timedTasks = new List<(float time, object tcs)>();
        
        public void AddTaskTimeout<T>(float time, TaskCompletionSource<T> task)
        {
            float targettime = secondsElapsed + time;

            if (usinProxy)
            {
                targettime *= 2;
            }

            if (!task.Task.IsCompleted)
                timedTasks.Add((targettime, task));
        }

        private void TryCancelTask(object task)
        {
            if (task is TaskCompletionSource<object>)
            {
                TaskCompletionSource<object> tcs = (TaskCompletionSource<object>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<string>)
            {
                TaskCompletionSource<string> tcs = (TaskCompletionSource<string>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<bool>)
            {
                TaskCompletionSource<bool> tcs = (TaskCompletionSource<bool>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<User>)
            {
                TaskCompletionSource<User> tcs = (TaskCompletionSource<User>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<CaseOpened>)
            {
                TaskCompletionSource<CaseOpened> tcs = (TaskCompletionSource<CaseOpened>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<Exchange>)
            {
                TaskCompletionSource<Exchange> tcs = (TaskCompletionSource<Exchange>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<PromoCodeSet>)
            {
                TaskCompletionSource<bool> tcs = (TaskCompletionSource<bool>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
            else if (task is TaskCompletionSource<PvpbattleCreated>)
            {
                TaskCompletionSource<PvpbattleCreated> tcs = (TaskCompletionSource<PvpbattleCreated>)task;

                if (!tcs.Task.IsCompleted)
                {
                    bool success = tcs.TrySetCanceled();
                    printToConsole($"Cancelled task: {task} - {success}");
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            if(secondsElapsed % 5 == 0)
            {
                //printToConsole($"Seconds elapsed: {secondsElapsed}");
            }

            List<(float time, object tcs)> listOfTasks = timedTasks;
            for (int i = 0; i < timedTasks.Count; i++)
            {
                var (time, task) = timedTasks[i];

                if (secondsElapsed >= time)
                {
                    TryCancelTask(task);

                        printToConsole($"Removed task: {timedTasks[i]} - {time} / {secondsElapsed}");
                    timedTasks.RemoveAt(i);
                }
            }
        }

        private void UpdateTaskTime()
        {
            if (retryInHour)
            {
                TimeSpan currentTime = DateTime.Now.TimeOfDay;
                TimeSpan oneHour = TimeSpan.FromHours(1);
                TimeSpan timePlusOneHour = currentTime.Add(oneHour);

                updateTaskSchedularTaskConfig(timePlusOneHour, false);
                return;
            }

            if(ConfigManager._Instance.GetConfigFile().triggerTime != new TimeSpan())
            {
                updateTaskSchedularTaskConfig(ConfigManager._Instance.GetConfigFile().triggerTime, false);
                return;
            }

            updateTaskSchedularTaskConfig(ConfigManager._Instance.GetScheduleTime(), true);
        }

        private void updateTaskSchedularTaskConfig(TimeSpan timeSpan, bool isUTC)
        {
            printToConsole("Updating task");
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskName);

                if (task != null)
                {
                    //Clear previous daily trigger so it can trigger in the same day
                    TaskDefinition taskDef = task.Definition;

                    if (!taskDef.Settings.StartWhenAvailable)
                    {
                        taskDef.Settings.StartWhenAvailable = true;
                    }

                    if(taskDef.Principal.RunLevel != TaskRunLevel.Highest)
                    {
                        if (IsRunningWithElevatedPrivileges())
                        {
                            taskDef.Principal.RunLevel = TaskRunLevel.Highest;
                        }
                    }


                    taskDef.Triggers.Clear();

                    DailyTrigger dailyTrigger = new DailyTrigger();
                    dailyTrigger.DaysInterval = 1;

                    DateTime now = isUTC ? DateTime.UtcNow : DateTime.Now;
                    DateTime triggerTime = (isUTC ? DateTime.UtcNow.Date : DateTime.Today) + timeSpan;

                    // If the trigger time has already passed today, move it to tomorrow
                    if (triggerTime <= now)
                    {
                        triggerTime = triggerTime.AddDays(1);
                    }

                    // Convert to local time if UTC was used
                    if (isUTC)
                    {
                        triggerTime = triggerTime.ToLocalTime();
                    }

                    dailyTrigger.StartBoundary = triggerTime;

                    taskDef.Triggers.Add(dailyTrigger);
                    taskDef.Actions.Clear();
                    taskDef.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location));

                    // Save the changes
                    task.RegisterChanges();
                }
                else
                {
                    createTaskSchedularTaskConfig(ts, timeSpan, isUTC);
                }
            }
        }

        private void createTaskSchedularTaskConfig(TaskService ts, TimeSpan timeSpan, bool isUTC)
        {
            printToConsole("Creating task");
            TaskDefinition taskDef = ts.NewTask();
            taskDef.RegistrationInfo.Description = "CSGORoll Daily Collector.";

            taskDef.Principal.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            taskDef.Principal.LogonType = TaskLogonType.InteractiveToken;
            taskDef.Principal.RunLevel = TaskRunLevel.Highest;

            DailyTrigger dailyTrigger = new DailyTrigger();
            taskDef.Settings.StartWhenAvailable = true;
            taskDef.Settings.WakeToRun = true;
            taskDef.Settings.DisallowStartIfOnBatteries = false;
            taskDef.Settings.RunOnlyIfIdle = false;

            if (IsRunningWithElevatedPrivileges())
            {
                // Set the task to run with elevated privileges
                taskDef.Principal.RunLevel = TaskRunLevel.Highest;
            }

            dailyTrigger.DaysInterval = 1;
            DateTime tt = DateTime.Today + timeSpan;

            DateTime now = isUTC ? DateTime.UtcNow : DateTime.Now;
            DateTime triggerTime = (isUTC ? DateTime.UtcNow.Date : DateTime.Today) + timeSpan;

            // If the trigger time has already passed today, move it to tomorrow
            if (triggerTime <= now)
            {
                triggerTime = triggerTime.AddDays(1);
            }

            // Convert to local time if UTC was used
            if (isUTC)
            {
                triggerTime = triggerTime.ToLocalTime();
            }

            taskDef.Triggers.Add(dailyTrigger);
            taskDef.Actions.Add(new ExecAction(Assembly.GetExecutingAssembly().Location));

            ts.RootFolder.RegisterTaskDefinition(taskName, taskDef);
        }

        private bool IsRunningWithElevatedPrivileges()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // Check if the current process is running as an administrator (elevated)
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        TaskCompletionSource<bool> _signalLoadStatus = null;
        TaskCompletionSource<User> _signalUserData = null;
        TaskCompletionSource<CaseOpened> _signalBoxOpened = null;
        TaskCompletionSource<Exchange> _signalSellInventory = null;
        TaskCompletionSource<PromoCodeSet> _signalPromoCode = null;
        TaskCompletionSource<PvpbattleCreated> _signalPvpBattle = null;
        TaskCompletionSource<bool> _signalSteamLoggedIn = null;
        public TaskCompletionSource<string> _signalSteamQRCode = null;
        public TaskCompletionSource<bool> _signalRequiresSteamCode = null;

        private void messagedReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                WebMessage message = JsonSerializer.Deserialize<WebMessage>(args.WebMessageAsJson);

                printToConsole($"Received message: {message}");

                switch (message?.type)
                {
                    case "UserData":
                        //signed in
                        User userdata = message.payload.Deserialize<User>();
                        _signalUserData.TrySetResult(userdata);
                        break;
                    case "UserDataNull":
                        //not signed in
                        _signalUserData.TrySetResult(null);
                        break;
                    case "LoadStatus":
                        _signalLoadStatus.TrySetResult(true);
                        //message = success
                        break;
                    case "BoxSlotsError":
                        printToConsole($"BoxOpenError: {message.payload}");
                        _signalBoxOpened.TrySetResult(null);
                        break;
                    case "BoxOpened":
                        CaseOpened caseopened = message.payload.Deserialize<CaseOpened>();
                        _signalBoxOpened.TrySetResult(caseopened);
                        break;
                    case "SellInventory":
                        Exchange exchange = message.payload.Deserialize<Exchange>();
                        _signalSellInventory.TrySetResult(exchange);
                        break;
                    case "SellInventoryError":
                        printToConsole($"SellInventoryError: {message.payload}");
                        _signalSellInventory.TrySetResult(null);
                        break;
                    case "PromoCodeError":
                        printToConsole($"PromoCodeError: {message.payload}");
                        _signalPromoCode.TrySetResult(null);
                        break;
                    case "PromoCodeSuccess":
                        PromoCodeSet promocodeset = message.payload.Deserialize<PromoCodeSet>();
                        _signalPromoCode.TrySetResult(promocodeset);
                        break;
                    case "PvpbattleCreated":
                        PvpbattleCreated pbc = message.payload.Deserialize<PvpbattleCreated>();
                        _signalPvpBattle.TrySetResult(pbc);
                        break;
                    case "PvpbattleCreatedError":
                        printToConsole($"PvpbattleCreatedError: {message.payload}");
                        _signalPvpBattle.TrySetResult(null);
                        break;
                    case "SteamSignedIn":
                        printToConsole($"Received: Logged into steam");
                        _signalSteamLoggedIn.TrySetResult(true);
                        break;
                    case "RequiresSteamGuardCode":
                        printToConsole($"Received: Requires steam guard code");
                        _signalRequiresSteamCode.TrySetResult(true);
                        break;
                    case "SteamQRCode":
                        QRCode qrc = message.payload.Deserialize<QRCode>();
                        printToConsole($"Received: Steam QR Code: {qrc.qrcode}");
                        _signalSteamQRCode.TrySetResult(qrc.qrcode);
                        break;
                    default:
                        printToConsole($"Received invalid message! {message}");
                        break;
                }

            } catch (Exception ex)
            {
                printToConsole($"MessageReceived Error: {ex.Message}");
            }
        }

        private int pvpbattleAttemps = 0;

        private async void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                printToConsole($"Navigation failed. Error: {e.WebErrorStatus}");

                switch (e.WebErrorStatus)
                {
                    case CoreWebView2WebErrorStatus.UnexpectedError:
                        printToConsole("Unexpected error while navigating.");
                        ReloadThePage(false);
                        break;
                    case CoreWebView2WebErrorStatus.HostNameNotResolved:
                        printToConsole("Provided host name was not able to be resolved.");
                        ReloadThePage(false);
                        break;
                    case CoreWebView2WebErrorStatus.CannotConnect:
                        printToConsole("A connection to the destination was not established.");
                        ReloadThePage(false);
                        break;
                    case CoreWebView2WebErrorStatus.Disconnected:
                        printToConsole("Internet connection has been lost.");
                        ReloadThePage(false);
                        break;
                    case CoreWebView2WebErrorStatus.Timeout:
                        printToConsole("The connection has timed out.");
                        ReloadThePage(false);
                        break;
                    case CoreWebView2WebErrorStatus.ServerUnreachable:
                        printToConsole("The host is unreachable.");
                        ReloadThePage(false);
                        break;
                    default:
                        printToConsole($"Unhandled navigation error: {e.WebErrorStatus}");
                        break;
                }
            }

            setURLTextbox(webView21.Source.ToString());
            printToConsole( $"Navigated to: {webView21.Source.ToString()}");
            string sourceString = webView21.Source.ToString();

            if (sourceString.Contains("steamcommunity.com")) {
                //login? -press sign in button
                //loginform -input username & password & steamguard code then redirects to login?
                if (sourceString.Contains("login?")){
                    //Already logged into steam
                    printToConsole($"Already logged into steam... pressing login button");

                    _signalSteamLoggedIn = new TaskCompletionSource<bool>();
                    AddTaskTimeout(15f, _signalSteamLoggedIn);

                    await webView21.ExecuteScriptAsync(new pressSignInButton().GetJavaScript());

                    try
                    {
                        await _signalSteamLoggedIn.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to login into steam!");
                        ReloadThePage(false);
                        return;
                    }

                    printToConsole("Successfully logged into steam");
                    
                } else if (sourceString.Contains("loginform"))
                {
                    printToConsole("User not logged into steam!");
                    // Notify discord bot

                    if(CommManager._Instance.enabled)
                        await CommManager._Instance.SendNotLoggedIn();

                    // Add a source check or something for steam guard after submitting steam login, w  ait x seconds then check
                }

            } else if (sourceString.Contains("csgoroll"))
            {
                //check if page is loaded
                await DelayAsyncSec(1f);
                printToConsole($"Checking if page is loaded");
                _signalLoadStatus = new TaskCompletionSource<bool>();
                AddTaskTimeout(15f, _signalLoadStatus);
                await webView21.ExecuteScriptAsync(new pageLoaded().GetJavaScript());

                try
                {
                    await _signalLoadStatus.Task;
                }
                catch (TaskCanceledException ex)
                {
                    printToConsole("Failed to load page after 15 seconds! Reloading...");
                    ReloadThePage();
                    return;
                }

                printToConsole($"Page is loaded");

                await DelayAsyncSec(1f);

                printToConsole($"Getting user data");

                //Get user data
                _signalUserData = new TaskCompletionSource<User>();
                AddTaskTimeout(30f, _signalUserData);
                await webView21.ExecuteScriptAsync(new getUserData().GetJavaScript());

                try
                {
                    await _signalUserData.Task;
                }
                catch (TaskCanceledException ex)
                {
                    printToConsole("Failed to get user data after 30 seconds! Reloading...");
                    ReloadThePage();
                    return;
                }

                if (_signalUserData.Task.Result == null)
                {
                    printToConsole("User is not logged into csgoroll! Redirecting...");
                    webView21.CoreWebView2.Navigate("https://steamcommunity.com/openid/loginform/?goto=%2Fopenid%2Flogin%3Fopenid.mode%3Dcheckid_setup%26openid.ns%3Dhttp%253A%252F%252Fspecs.openid.net%252Fauth%252F2.0%26openid.identity%3Dhttp%253A%252F%252Fspecs.openid.net%252Fauth%252F2.0%252Fidentifier_select%26openid.claimed_id%3Dhttp%253A%252F%252Fspecs.openid.net%252Fauth%252F2.0%252Fidentifier_select%26openid.return_to%3Dhttps%253A%252F%252Fapi.entertoroll.com%252Fauth%252Fsteam%252Freturn%26openid.realm%3Dhttps%253A%252F%252Fapi.entertoroll.com");
                    return;
                }

                User userData = _signalUserData.Task.Result;
                CaseIDManager._Instance.GotPlayerLevel(userData.level);
                ConfigManager._Instance.SetScheduleTime(userData.dailyFreeTimeSlot);

                await DelayAsyncSec(1f);

                List<string> cidpvp = CaseIDManager._Instance.caseIdsToPvpBattle;

                //create pvp battle
                if (cidpvp.Count > 0)
                {
                    printToConsole($"Creating pvp battle...");

                    var boxes = new List<object>();

                    for(int i = 0; i < cidpvp.Count; i++)
                    {
                        boxes.Add(new { boxId = cidpvp[i], roundNumber = i });
                    }

                    string boxesJson = JsonSerializer.Serialize(boxes);

                    printToConsole($"Executing pvp battle...");

                    pvpbattleAttemps++;

                    printToConsole($"Executing pvp battle... 1");

                    _signalPvpBattle = new TaskCompletionSource<PvpbattleCreated>();
                    AddTaskTimeout(7f, _signalPvpBattle);

                    printToConsole($"Executing pvp battle... 2");

                    string script = new createPvPGame().GetJavaScript(new Dictionary<string, string>
                    {
                        ["boxes"] = boxesJson,
                        ["strategy"] = ConfigManager._Instance.GetConfigFile().pvpStrategy,
                        ["mode"] = ConfigManager._Instance.GetConfigFile().pvpMode,
                        ["playercount"] = ConfigManager._Instance.GetConfigFile().pvpNumOfPlayers.ToString(),
                        ["teamcount"] = ConfigManager._Instance.GetConfigFile().pvpNumOfTeams.ToString(),
                        ["teamplayerscount"] = ConfigManager._Instance.GetConfigFile().pvpNumOfPlayersOnTeam.ToString()
                    });

                    printToConsole(script);

                    await webView21.ExecuteScriptAsync(script);

                    printToConsole($"Executing pvp battle... 3");

                    try
                    {
                        await _signalPvpBattle.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to execute pvp battle");
                    }

                    if (_signalPvpBattle.Task.IsCompleted)
                    {
                        printToConsole($"Executing pvp battle... 5");
                        if (_signalPvpBattle.Task.Result != null)
                        {
                            PvpbattleCreated pbc = _signalPvpBattle.Task.Result;
                            if (pbc.createPvpGame == null)
                            {
                                printToConsole($"Failed creating pvp battle, one of the cases was already unboxed!");
                                if (pvpbattleAttemps <= 3)
                                {
                                    printToConsole($"Retrying...");
                                    if (ReloadThePage())
                                        return;
                                }
                                printToConsole($"Failed creating pvp battle too many times");
                            } else
                            {
                                printToConsole($"Successfully created pvp battle");
                            }

                            printToConsole($"Executing pvp battle... 6");

                            CaseIDManager._Instance.caseIdsToPvpBattle.Clear();
                        } else
                        {
                            await DelayAsyncSec(10f);
                            printToConsole($"Failed creating pvp battle...");
                            //failed to create battle
                            if (pvpbattleAttemps <= 3)
                            {
                                if (ReloadThePage())
                                    return;
                            }
                            printToConsole($"Failed creating pvp battle too many times");
                        }
                    }

                    await DelayAsyncSec(5f);
                }
                

                printToConsole($"Opening cases");

                //open cases
                List<string> localCasesToOpen = new List<string>(CaseIDManager._Instance.caseIdsToOpen);

                foreach(string bid in localCasesToOpen)
                {
                    printToConsole($"Opening box: {bid}");
                    _signalBoxOpened = new TaskCompletionSource<CaseOpened>();
                    AddTaskTimeout(5f, _signalBoxOpened);
                    printToConsole($"Sending script");
                    string script1 = new openCaseFull().GetJavaScript(new Dictionary<string, string> { ["boxId"] = bid });

                    await webView21.ExecuteScriptAsync(script1);

                    printToConsole($"Script sent, waiting for reply...");

                    printToConsole(script1);

                    try
                    {
                        await _signalBoxOpened.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to open box");
                    }

                    if (_signalBoxOpened.Task.IsCompleted)
                    {
                        //success
                        if(_signalBoxOpened.Task.Result != null)
                        {
                            //opened box
                            CaseOpened co = _signalBoxOpened.Task.Result;

                            if (co != null && co.data != null)
                            {
                                if (co.data.openBox != null)
                                {
                                    printToConsole($"Successfully opened box: {bid} - {co.data.openBox}");
                                    CaseIDManager._Instance.caseIdsToOpen.Remove(bid); //remove from list of cases we need to open
                                    CaseIDManager._Instance.openedCasesResults.Add(co);
                                    await DelayAsyncSec(5f);
                                    //if (ReloadThePage(false)) //try refresh data
                                    //    return;
                                    continue;
                                } else
                                {
                                    if(co.errors != null && co.errors.Count > 0)
                                    {
                                        //Ran into an error
                                        printToConsole($"CSGORoll Error: {co.errors[0].message}");

                                        if (co.errors != null && co.errors.Count > 0 && co.errors[0].message.Contains("Box in use"))
                                        {
                                            printToConsole($"CSGORoll Error: Box in use");
                                        }
                                        else if (co.errors != null && co.errors.Count > 0 && co.errors[0].message.Contains("Balance on slot has changed"))
                                        {
                                            printToConsole($"CSGORoll Error: Balance on slot has changed");
                                        }
                                        else if (co.errors != null && co.errors.Count > 0 && co.errors[0].message.Contains("GEO_BLOCK"))
                                        {
                                            printToConsole($"CSGORoll Error: GEO Block");
                                        }
                                        else if (co.errors != null && co.errors.Count > 0 && co.errors[0].message.Contains("cannot open more"))
                                        {
                                            printToConsole($"Case was already opened: {bid} - {co.data.openBox}");
                                            CaseIDManager._Instance.caseIdsToOpen.Remove(bid); //remove from list of cases we need to open
                                            await DelayAsyncSec(5f);
                                            //if (ReloadThePage(false)) //try refresh data
                                            //    return;
                                            continue;
                                        }
                                        else
                                        {
                                            printToConsole($"CSGORoll Error: Unhandled Error!");
                                        }

                                    }
                                }
                            }
                            
                        }
                        
                    }

                    //failed
                    printToConsole($"Failed to open box: {bid}");
                    await DelayAsyncSec(5f);
                }

                printToConsole($"Inital case opening finished...");
                await DelayAsyncSec(5f);

                if (CaseIDManager._Instance.caseIdsToOpen.Count > 0)
                {
                    //Some cases failed to open, refresh the page and try again
                    if (ReloadThePage())
                        return;
                }

                await DelayAsyncSec(1f);
                printToConsole($"Selling items");
                _signalSellInventory = new TaskCompletionSource<Exchange>();
                AddTaskTimeout(20f, _signalSellInventory);
                await webView21.ExecuteScriptAsync(new sellInventoryItems().GetJavaScript());
                try
                {
                    await _signalSellInventory.Task;
                }
                catch (TaskCanceledException ex)
                {
                    printToConsole($"Failed to sell items - {ex.ToString()}");
                    if (ReloadThePage())
                        return;
                }

                if (!_signalSellInventory.Task.IsCompleted)
                {
                    printToConsole($"Failed to sell items");
                    if (ReloadThePage())
                        return;
                }

                await DelayAsyncSec(1f);

                if(ConfigManager._Instance.GetConfigFile().disableAffiliate != "Yes :(")
                {
                    printToConsole($"Setting affiliate code");

                    //set affiliate code
                    _signalPromoCode = new TaskCompletionSource<PromoCodeSet>();
                    AddTaskTimeout(10f, _signalPromoCode);
                    await webView21.ExecuteScriptAsync(new setPromoCode().GetJavaScript());

                    try
                    {
                        await _signalPromoCode.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to set promocode");
                    }

                    if (!_signalPromoCode.Task.IsCompleted)
                    {
                        printToConsole("Failed setting promocode!");
                        if (ReloadThePage())
                            return;
                    }

                    PromoCodeSet pcs = _signalPromoCode.Task.Result;

                    if (pcs == null)
                    {
                        printToConsole("Failed setting promocode!");
                        if (ReloadThePage())
                            return;
                    } else
                    {
                        printToConsole($"Successfully set promocode. {pcs.data}");
                    }
                        
                } else
                {
                    printToConsole($"Skipping setting promocode :(");
                }


                //send webhook message

                if (ConfigManager._Instance.GetConfigFile().enableDiscordWebhook && CaseIDManager._Instance.openedCasesResults.Count > 0)
                {
                    //try to get the user's new balance
                    //Get user data
                    _signalUserData = new TaskCompletionSource<User>();
                    AddTaskTimeout(10f, _signalUserData);
                    await webView21.ExecuteScriptAsync(new getUserData().GetJavaScript());

                    try
                    {
                        await _signalUserData.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to get user data");
                    }

                    if (_signalUserData.Task.IsCanceled)
                    {
                        printToConsole("Failed to get user data for webhook!");
                    }
                    else if (_signalUserData.Task.Result == null)
                    {
                        printToConsole("User is not logged into csgoroll when grabbing for webhook!");
                    }
                    else
                    {
                        userData = _signalUserData.Task.Result;
                    }

                    printToConsole("Sending discord webhook message...");
                    await SendDiscordWebHook(userData);
                }

                if (CommManager._Instance.enabled)
                {
                    _signalUserData = new TaskCompletionSource<User>();
                    AddTaskTimeout(10f, _signalUserData);
                    await webView21.ExecuteScriptAsync(new getUserData().GetJavaScript());

                    try
                    {
                        await _signalUserData.Task;
                    }
                    catch (TaskCanceledException ex)
                    {
                        printToConsole("Failed to get user data");
                    }

                    if (_signalUserData.Task.IsCanceled)
                    {
                        printToConsole("Failed to get user data for webhook!");
                    }
                    else if (_signalUserData.Task.Result == null)
                    {
                        printToConsole("User is not logged into csgoroll when grabbing for webhook!");
                    }
                    else
                    {
                        userData = _signalUserData.Task.Result;
                    }

                    await SendDiscordBotInfo(userData);
                }

                printToConsole($"Updating task time...");
                UpdateTaskTime();
                quitWebBrowser();
            }

        }

        private async System.Threading.Tasks.Task SendDiscordBotInfo(User userdata)
        {
            if (CaseIDManager._Instance.openedCasesResults.Count == 0)
            {
                var datanocase = new
                {
                    command = "collected",
                    clientId = CommManager._Instance.clientID,
                    title = "No cases were ready to be claimed!",
                    description = $"Account Name: `{userdata.name}`\n" +
                        $"Balance: `{CaseIDManager._Instance.GetPlayerMainWalletBalance(userdata)}`\n\n",
                };

                await CommManager._Instance.SendNotification(datanocase);
                return;
            }

            CaseOpened bestrolled = CaseIDManager._Instance.GetBestItemRolled();
            caseOpenedBoxOpening brbo = bestrolled.data.openBox.boxOpenings[0];
            var brcase = CaseIDManager._Instance.GetLevelPercent(bestrolled.data.openBox.box.id);
            CaseOpened bestprofit = CaseIDManager._Instance.GetMostValuableItemUnboxed();
            caseOpenedBoxOpening bpbo = bestprofit.data.openBox.boxOpenings[0];
            var bpcase = CaseIDManager._Instance.GetLevelPercent(bestprofit.data.openBox.box.id);

            var data = new
            {
                command = "collected",
                clientId = CommManager._Instance.clientID,
                title = "Your cases have been claimed!",
                description = $"Account Name: `{userdata.name}`\n" +
                        $"Balance: `{CaseIDManager._Instance.GetPlayerMainWalletBalance(userdata)}`\n\n" +
                        $"__Most valuable item unboxed__\n" +
                        $"Case: Level {bpcase.level} - {bpcase.percent}%\n" +
                        $"{bpbo.userItem.itemVariant.brand} - {brbo.userItem.itemVariant.name}\n" +
                        $"Roll: {bpbo.roll.value}\n" +
                        $"Value: {bpbo.userItem.itemVariant.value}\n\n" +
                        $"__Best item rolled__\n" +
                        $"Case: Level {brcase.level} - {brcase.percent}%\n" +
                        $"{brbo.userItem.itemVariant.brand} - {brbo.userItem.itemVariant.name}\n" +
                        $"Roll: {brbo.roll.value}\n" +
                        $"Value: {brbo.userItem.itemVariant.value}\n\n",
                /*color = 0xfb2b23,
                timestamp = DateTime.UtcNow.ToString("o"),
                footer = new
                {
                    text = "https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot"
                },
                thumbnail = new
                {
                    url = "https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp"
                },
                author = new
                {
                    name = "CSGORoll Daily Cases Collector",
                    url = "https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot",
                    icon_url = "https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp"
                }*/
            };

            await CommManager._Instance.SendNotification(data);
        }

        private async System.Threading.Tasks.Task SendDiscordWebHook(User userdata)
        {
            string webhookURL = ConfigManager._Instance.GetConfigFile().discordWebhookURL;

            CaseOpened bestrolled = CaseIDManager._Instance.GetBestItemRolled();
            caseOpenedBoxOpening brbo = bestrolled.data.openBox.boxOpenings[0];
            var brcase = CaseIDManager._Instance.GetLevelPercent(bestrolled.data.openBox.box.id);
            CaseOpened bestprofit = CaseIDManager._Instance.GetMostValuableItemUnboxed();
            caseOpenedBoxOpening bpbo = bestprofit.data.openBox.boxOpenings[0];
            var bpcase = CaseIDManager._Instance.GetLevelPercent(bestprofit.data.openBox.box.id);

            var payload = new
            {
                username = "CSGORoll Daily Collector",
                avatar_url = "https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp",
                embeds = new[]
                {
                    new
                    {
                        title = "Your cases have been claimed!",
                        description = $"Account Name: `{userdata.name}`\n" +
                        $"Balance: `{CaseIDManager._Instance.GetPlayerMainWalletBalance(userdata)}`\n\n" +
                        $"__Most valuable item unboxed__\n" +
                        $"Case: Level {bpcase.level} - {bpcase.percent}%\n" +
                        $"{bpbo.userItem.itemVariant.brand} - {brbo.userItem.itemVariant.name}\n" +
                        $"Roll: {bpbo.roll.value}\n" +
                        $"Value: {bpbo.userItem.itemVariant.value}\n\n" +
                        $"__Best item rolled__\n" +
                        $"Case: Level {brcase.level} - {brcase.percent}%\n" +
                        $"{brbo.userItem.itemVariant.brand} - {brbo.userItem.itemVariant.name}\n" +
                        $"Roll: {brbo.roll.value}\n" +
                        $"Value: {brbo.userItem.itemVariant.value}\n\n",
                        color = 0xfb2b23,
                        timestamp = DateTime.UtcNow.ToString("o"),
                        footer = new
                        {
                            text = "https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot"
                        },
                        thumbnail = new
                        {
                            url = "https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp"
                        },
                        author = new
                        {
                            name = "CSGORoll Daily Cases Collector",
                            url = "https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot",
                            icon_url = "https://cdn.discordapp.com/avatars/1276929592866640014/03b5f7449deae1bd9863657ecb73a4ae.webp"
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });

            var client = new HttpClient();
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(webhookURL, content);
            printToConsole(response.IsSuccessStatusCode ? "Successfully sent webhook message" : "Failed to send discord webhook");
        }

        private CancellationTokenSource _cancellationTokenSource;

        private int timesReloadedPage = 0;

        private bool retryInHour = false;

        private bool ReloadThePage(bool increaseTimer = true)
        {
            if(timesReloadedPage > 15 && !ConfigManager._Instance.GetConfigFile().keepRetryingAfterFail)
            {
                printToConsole("Reloaded the page too many times! Trying to continue to execute the rest and attempt again in 1 hour...");
                retryInHour = true;
                return false;
            }

            if(increaseTimer)
                timesReloadedPage++;

            webView21.Reload();
            return true;
        }

        public void quitWebBrowser()
        {
            printToConsole($"Attempting to quit...");
            if (!disableAutoQuit)
            {
                quittingApplication = true;
                Application.Exit();
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

                    getWebBrowserCommand(command)?.executeCommand(command, args);
                }
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}