using System.Net.Sockets;
using System.Text;
using FluentAssertions;

namespace SensorsHub.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestTcpServerResponse()
        {
            // Arrange - Start your server
            // Ensure your server is listening before running the test
            int port = 5000;  // Make sure this matches your server configuration
            string hostname = "127.0.0.1";
            var client = new TcpClient();

            try
            {
                // Act - Connect to the server
                await client.ConnectAsync(hostname, port);
                var stream = client.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                var reader = new StreamReader(stream, Encoding.UTF8);

                // Send a JSON string as test data
                var testData = "{\"id\": \"device123\", \"temp\": 23.5}";
                await writer.WriteLineAsync(testData);

                // Read the response from the server
                var response = await reader.ReadLineAsync();

                // Assert - Validate the response
                response.Should().NotBeNullOrEmpty();
                // Example: Expecting the server to echo back the data or some acknowledgment
                response.Should().BeEquivalentTo("Some expected response based on server logic");

            }
            finally
            {
                // Clean up
                if (client != null && client.Connected)
                {
                    client.Close();
                }
            }
        }
    }
}