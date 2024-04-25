using SensorsHub.ControllersTCP;

namespace SensorsHub
{
    public class TcpServerHostedService : BackgroundService
    {
        private TcpServer _server;

        public TcpServerHostedService()
        {
            _server = new TcpServer("127.0.0.1", 12345); // Set your TCP server's IP and port
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.StartAsync(); // Start server in background
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Stop();
            await base.StopAsync(cancellationToken);
        }
    }
}
