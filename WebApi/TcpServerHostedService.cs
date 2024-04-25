using SensorsHub.ControllersTCP;

namespace SensorsHub
{
    public class TcpServerHostedServiceReal : BackgroundService
    {
        private TcpServer _server;

        public TcpServerHostedServiceReal()
        {
            _server = new TcpServer("127.0.0.1", 5000); // Set your TCP server's IP and port
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
