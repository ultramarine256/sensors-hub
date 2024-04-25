using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;

namespace SensorsHub.ControllersTCP
{
    public class LiveDevice : IDisposable
    {
        public TcpClient Client { get; private set; }
        public string DeviceId { get; private set; }
        public NetworkStream Stream { get; private set; }
        private StreamReader reader;
        private StreamWriter writer;
        private Timer keepAliveTimer;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public LiveDevice(TcpClient client)
        {
            Client = client;
            Stream = client.GetStream();
            reader = new StreamReader(Stream, Encoding.UTF8);
            writer = new StreamWriter(Stream, Encoding.UTF8) { AutoFlush = true };
            DeviceId = "Unknown (" + ((IPEndPoint)client.Client.RemoteEndPoint).Address + ":" + ((IPEndPoint)client.Client.RemoteEndPoint).Port + ")";
            // TODO: implement later
            // Client.Client.SetKeepAlive(15000, 1000); // Keep-alive interval set for 15 seconds with retries every second
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 65000); // Socket timeout of 65 seconds

            SetupHelloTimeout(5000);
        }

        private void SetupHelloTimeout(int timeout)
        {
            var helloTimeout = new Timer((state) =>
            {
                Console.WriteLine("No 'hello!' received from " + DeviceId);
                Disconnect();
            }, null, timeout, Timeout.Infinite);
        }

        public async Task ProcessIncomingMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && Client.Connected)
                {
                    string line = await reader.ReadLineAsync();
                    if (line == "Hello!")
                    {
                        Console.WriteLine("Got hello from " + DeviceId);
                        continue;
                    }
                    ProcessLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error from " + DeviceId + ": " + ex.Message);
            }
        }

        private void ProcessLine(string line)
        {
            try
            {
                var data = JsonSerializer.Deserialize<dynamic>(line); // Assuming line is a valid JSON string
                if (data != null && data.ContainsKey("id") && data.id is string)
                {
                    string oldId = DeviceId;
                    DeviceId = data.id.ToLower();
                    Console.WriteLine($"Device '{oldId}' is now known as '{DeviceId}'");
                }
                Console.WriteLine("[OUT] " + JsonSerializer.Serialize(data));
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON decode error: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            Client.Close();
            Console.WriteLine("Device " + DeviceId + " disconnected...");
        }

        public void Dispose()
        {
            reader?.Dispose();
            writer?.Dispose();
            Stream?.Dispose();
            Client?.Dispose();
        }
    }

    public class TcpServer
    {
        private TcpListener listener;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public TcpServer(string hostname, int port)
        {
            listener = new TcpListener(IPAddress.Parse(hostname), port);
        }

        public async Task StartAsync()
        {
            listener.Start();
            Console.WriteLine("Server listening...");

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("New connection: " + client.Client.RemoteEndPoint.ToString());
                    var device = new LiveDevice(client);
                    Task.Run(() => device.ProcessIncomingMessagesAsync(cts.Token));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server error: " + ex.Message);
            }
        }

        public void Stop()
        {
            cts.Cancel();
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
