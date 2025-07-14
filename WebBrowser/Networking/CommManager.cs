using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebBrowser.Config;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.actions;
using WebBrowser.WebBrowserJavaScriptInjections.scripts.steam;

namespace WebBrowser.Networking
{
    class CommManager
    {
        public static CommManager _Instance;

        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 5695;

        public string clientID = "";

        public bool enabled = false;

        private TcpClient tcpClient;
        private NetworkStream stream;

        public CommManager()
        {
            _Instance = this;
            clientID = ConfigManager._Instance.GetConfigFile().identifier;

            enabled = ConfigManager._Instance.GetConfigFile().enableLocalBotComm;

            if (!enabled) return;

            Task.Run(() => StartAsync())
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                    Console.WriteLine("TCP client failed: " + t.Exception.InnerException?.Message);
            });
        }

        public async Task StartAsync()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ServerAddress, ServerPort);
            Console.WriteLine("Connected to server.");

            stream = tcpClient.GetStream();

            int procid = Process.GetCurrentProcess().Id;

            // Send initial handshake
            var handshake = new { 
                clientId = clientID, 
                pid = procid 
            };

            await SendJsonAsync(handshake);

            // Enter main loop to receive and respond
            await ListenLoop();
        }

        private async Task ListenLoop()
        {
            var buffer = new byte[4096];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                
                if (bytesRead == 0)
                {
                    Form1._instance.printToConsole("Server closed connection.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Form1._instance.printToConsole($"Received from server: {message}");

                // Parse JSON and handle "msg"
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(message);

                    if (json.TryGetProperty("command", out JsonElement cmd))
                    {
                        string cmdstr = cmd.ToString();
                        Form1 finst = Form1._instance;
                        Microsoft.Web.WebView2.WinForms.WebView2 wv2Inst = finst.GetWebviewObj();
                        string sourceString = wv2Inst.Source.ToString();

                        Form1._instance.printToConsole($"Command: {cmdstr}");

                        switch (cmdstr)
                        {
                            case "loginSteamDetails":
                                if(sourceString.Contains("steamcommunity.com") && sourceString.Contains("loginform"))
                                {
                                    if(json.TryGetProperty("susername", out JsonElement steamusername) && json.TryGetProperty("spassword", out JsonElement steampassword))
                                    {
                                        string script = new inputUsernamePassword().GetJavaScript(new Dictionary<string, string>
                                        {
                                            ["username"] = steamusername.GetString(),
                                            ["password"] = steampassword.GetString(),
                                        });

                                        finst._signalRequiresSteamCode = new TaskCompletionSource<bool>();
                                        finst.AddTaskTimeout(15f, finst._signalRequiresSteamCode);

                                        finst.printToConsole($"Executing login script: {script}");

                                        wv2Inst.Invoke(new Action(async () =>
                                        {
                                            await wv2Inst.ExecuteScriptAsync(script);
                                        }));

                                        finst.printToConsole("Finished executing script");

                                        try
                                        {
                                            await finst._signalRequiresSteamCode.Task;

                                            finst.printToConsole("Sending Steam guard code request...");
                                            await SendRequireSteamGuard();
                                            finst.printToConsole("Sent Steam guard code request");
                                            continue;

                                        } catch (TaskCanceledException ex)
                                        {
                                            finst.printToConsole("Failed to login! Possibly incorrect login details! Reloading...");
                                            await SendIncorrectSteamLogin();
                                            //await SendErrorMessage("Failed to login! Possibly incorrect login details!");
                                            wv2Inst.Invoke(new Action(async () =>
                                            {
                                                wv2Inst.Reload();
                                            }));
                                            continue;
                                        }

                                        //finst.printToConsole("Finished user pass");

                                        /*if (finst._signalRequiresQRCode.Task.IsCanceled)
                                        {
                                            finst.printToConsole("Failed to login! Possibly incorrect login details! Reloading...");
                                            //await SendErrorMessage("Failed to login! Possibly incorrect login details!");
                                            wv2Inst.Reload();
                                            return;
                                        } else
                                        {
                                            finst.printToConsole("Task success");
                                        }*/
                                    }
                                } else
                                {
                                    finst.printToConsole("Not ready to login");
                                    await SendErrorMessage("Not ready to login");
                                }
                                break;
                            case "enterSteamGuard":
                                if (sourceString.Contains("steamcommunity.com") && sourceString.Contains("loginform"))
                                {
                                    if (json.TryGetProperty("scode", out JsonElement scode))
                                    {
                                        string script = new inputSteamGuardCode().GetJavaScript(new Dictionary<string, string>
                                        {
                                            ["steamcode"] = scode.GetString(),
                                        });

                                        finst._signalRequiresSteamCode = new TaskCompletionSource<bool>();
                                        finst.AddTaskTimeout(15f, finst._signalRequiresSteamCode);

                                        wv2Inst.Invoke(new Action(async () =>
                                        {
                                            await wv2Inst.ExecuteScriptAsync(script);
                                        }));

                                        try
                                        {
                                            await finst._signalRequiresSteamCode.Task;
                                        }
                                        catch (TaskCanceledException ex)
                                        {
                                            finst.printToConsole("Failed to login! Reloading...");
                                            await SendIncorrectSteamGuard();
                                            //await SendErrorMessage("Failed to login! Possibly incorrect login details!");
                                            wv2Inst.Invoke(new Action(async () =>
                                            {
                                                wv2Inst.Reload();
                                            }));
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    await SendErrorMessage("Not ready for steam guard code");
                                }
                                break;
                            case "requestQRCode":
                                // check if ready to login, get and send qr code
                                if (sourceString.Contains("steamcommunity.com") && sourceString.Contains("loginform"))
                                {
                                    finst._signalSteamQRCode = new TaskCompletionSource<string>();
                                    finst.AddTaskTimeout(15f, finst._signalSteamQRCode);
                                    
                                    wv2Inst.Invoke(new Action(async () =>
                                    {
                                        await wv2Inst.ExecuteScriptAsync(new getSteamLoginQRCodeFunction().GetJavaScript());
                                    }));

                                    try
                                    {
                                        await finst._signalSteamQRCode.Task;

                                        string qrc = finst._signalSteamQRCode.Task.Result;

                                        if(qrc != null)
                                        {
                                            await SendQrCode(qrc);
                                            finst.printToConsole("Sent QR Code");
                                        }
                                    }
                                    catch (TaskCanceledException ex)
                                    {
                                        finst.printToConsole("Failed to get QR code after 15 seconds! Reloading...");
                                        await SendErrorMessage("Failed to get QR code!");
                                        //await SendErrorMessage("Failed to login! Possibly incorrect login details!");
                                        wv2Inst.Invoke(new Action(async () =>
                                        {
                                            wv2Inst.Reload();
                                        }));
                                        continue;
                                    }
                                }
                                else
                                {
                                    await SendErrorMessage("Not ready to login");
                                }
                                break;
                            case "clearAllCookies":
                                Form1._instance.GetWebviewObj().CoreWebView2.CookieManager.DeleteAllCookies();
                                await SendMessage("Cleared cookies");
                                break;
                            case "quit":
                                await SendMessage("Quitting program...");
                                Form1._instance.quitWebBrowser();
                                break;
                            default:
                                finst.printToConsole("Unknown command");
                                break;
                        }
                    } 
                    else if (json.TryGetProperty("error", out var errorMsg))
                    {
                        Form1._instance.printToConsole($"Received error message from bot: {errorMsg}");
                    }
                    else if (json.TryGetProperty("msg", out var msgProp))
                    {
                        string receivedMsg = msgProp.GetString();
                        Form1._instance.printToConsole($"Message from bot: {receivedMsg}");

                        // Respond with a message
                        var response = new { msg = $"Reply from {clientID}: Got your message: '{receivedMsg}'" };
                        await SendJsonAsync(response);
                    } else
                    {
                        Form1._instance.printToConsole($"Received unknown message from bot: {message}");
                    }
                }
                catch (JsonException)
                {
                    Form1._instance.printToConsole("Received invalid JSON.");
                }
            }
        }

        private async Task SendJsonAsync(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task SendNotification(object dataToSend)
        {
            await SendJsonAsync(dataToSend);
        }

        public async Task SendNotLoggedIn()
        {
            var data = new
            {
                command = "notLoggedIn",
                clientId = clientID,
            };

            await SendJsonAsync(data);
        }

        public async Task SendQrCode(string qr)
        {
            var data = new
            {
                command = "qrCode",
                clientId = clientID,
                qrcode = qr,
            };

            await SendJsonAsync(data);
        }

        public async Task SendRequireSteamGuard()
        {
            var data = new
            {
                command = "requireSteamGuard",
                clientId = clientID,
            };

            await SendJsonAsync(data);
        }

        public async Task SendIncorrectSteamLogin()
        {
            var data = new
            {
                command = "incorrectSteamLogin",
                clientId = clientID,
            };

            await SendJsonAsync(data);
        }

        public async Task SendIncorrectSteamGuard()
        {
            var data = new
            {
                command = "incorrectSteamGuard",
                clientId = clientID,
            };

            await SendJsonAsync(data);
        }

        public async Task SendCollected()
        {
            var data = new
            {
                command = "collected",
                clientId = clientID,
                // SEND ALL THE EMBED DATA THAT WE WOULD NORMALLY SEND VIA WEBHOOK
            };

            await SendJsonAsync(data);
        }

        public async Task SendLoggedIn()
        {
            var data = new
            {
                command = "loggedIn",
                clientId = clientID,
            };

            await SendJsonAsync(data);
        }

        public async Task SendErrorMessage(string message)
        {
            var data = new
            {
                command = "errorMessage",
                clientId = clientID,
                errorMessage = message,
            };

            await SendJsonAsync(data);
        }

        public async Task SendMessage(string message)
        {
            var data = new
            {
                command = "message",
                clientId = clientID,
                message = message,
            };

            await SendJsonAsync(data);
        }
    }
}
