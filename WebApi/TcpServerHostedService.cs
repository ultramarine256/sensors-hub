using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace WebApi
{
    public class TcpServerHostedService : IHostedService
    {
        private readonly ILogger<TcpServerHostedService> _logger;
        private TcpListener _listener;
        private bool _isRunning;
        private ConcurrentDictionary<string, TcpClient> _connections = new ConcurrentDictionary<string, TcpClient>();

        public TcpServerHostedService(ILogger<TcpServerHostedService> logger)
        {
            _logger = logger;
            _listener = new TcpListener(IPAddress.Loopback, 5000);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _isRunning = true;
            _listener.Start();
            _logger.LogInformation("TCP Server started on {0}", _listener.LocalEndpoint);
            Task.Run(() => RunServer(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isRunning = false;
            _listener.Stop();
            _logger.LogInformation("TCP Server stopped.");
            return Task.CompletedTask;
        }

        private async Task RunServer(CancellationToken cancellationToken)
        {
            try
            {
                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.LogInformation("Connected to client {0}", client.Client.RemoteEndPoint);
                    HandleClient(client, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running TCP server");
            }
        }

        private async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
        {
            var endPoint = client.Client.RemoteEndPoint.ToString();
            _connections.TryAdd(endPoint, client);
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Received from {0}: {1}", endPoint, line);
                        ProcessCommand(line, writer, endPoint);
                    }
                }
            }
            finally
            {
                TcpClient removedClient;
                _connections.TryRemove(endPoint, out removedClient);
                _logger.LogInformation("Client disconnected: {0}", endPoint);
            }
        }

        private void ProcessCommand(string command, StreamWriter writer, string deviceId)
        {
            // Process and respond to commands, e.g., echoing back or handling specific commands
            if (command.StartsWith("u"))
            {
                writer.WriteLine("Update command received");
            }
            else
            {
                writer.WriteLine($"Echo: {command}");
            }
        }
    }
}
