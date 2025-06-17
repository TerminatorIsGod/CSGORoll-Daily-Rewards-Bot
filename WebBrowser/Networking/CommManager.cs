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

namespace WebBrowser.Networking
{
    class CommManager
    {
        public static CommManager _Instance;

        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 5695;

        private string clientID = "";

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
                    Console.WriteLine("Server closed connection.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from server: {message}");

                // Parse JSON and handle "msg"
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(message);

                    if (json.TryGetProperty("command", out JsonElement cmd))
                    {
                        string cmdstr = cmd.ToString();
                        switch (cmdstr)
                        {
                            case "loginSteamDetails":
                                break;
                            case "enterSteamGuard":
                                break;
                            case "requestQRCode":
                                break;
                            case "clearCookies":
                                break;
                            case "quit":
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
                        Console.WriteLine($"Message from bot: {receivedMsg}");

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
                    Console.WriteLine("Received invalid JSON.");
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
    }
}
