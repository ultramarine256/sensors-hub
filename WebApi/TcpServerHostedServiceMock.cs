using System.Net.Sockets;
using System.Net;

namespace WebApi
{
    public class TcpServerHostedService : IHostedService
    {
        private readonly ILogger<TcpServerHostedService> _logger;
        private TcpListener _listener;
        private bool _isRunning;

        public TcpServerHostedService(ILogger<TcpServerHostedService> logger)
        {
            _logger = logger;
            _listener = new TcpListener(IPAddress.Loopback, 80); // Ensure the port matches your test setup
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _isRunning = true;
            _listener.Start();
            _logger.LogInformation("TCP Server started on {0}", _listener.LocalEndpoint);
            Task.Run(() => RunServer(cancellationToken));
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
                    HandleClient(client);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running TCP server");
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            {
                var stream = client.GetStream();
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                string request = reader.ReadLine();
                _logger.LogInformation("Received: {0}", request);
                writer.WriteLine("Echo K: " + request);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _isRunning = false;
            _listener.Stop();
            _logger.LogInformation("TCP Server stopped.");
            return Task.CompletedTask;
        }
    }
}
