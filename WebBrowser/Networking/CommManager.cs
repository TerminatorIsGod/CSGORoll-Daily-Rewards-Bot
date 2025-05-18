using System;
using System.Collections.Generic;
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

            // Send initial handshake
            var handshake = new { clientId = clientID };
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

                    if (json.TryGetProperty("msg", out var msgProp))
                    {
                        string receivedMsg = msgProp.GetString();
                        Console.WriteLine($"Message from bot: {receivedMsg}");

                        // Respond with a message
                        var response = new { msg = $"Reply from {clientID}: Got your message: '{receivedMsg}'" };
                        await SendJsonAsync(response);
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
    }
}
