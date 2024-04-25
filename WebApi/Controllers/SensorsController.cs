using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using SensorsHub.Domain;

namespace SensorsHub.Controllers
{
    [ApiController]
    [Route("hub")]
    public class SensorsController : ControllerBase
    {
        private readonly ILogger<SensorsController> _logger;
        private static ConcurrentDictionary<string, Device> _connections = new ConcurrentDictionary<string, Device>();

        public SensorsController(ILogger<SensorsController> logger)
        {
            _logger = logger;
            // Initialize connections with demo data
            _connections.TryAdd("device123", new Device { DeviceId = "device123", IsOnline = true });
            _connections.TryAdd("device456", new Device { DeviceId = "device456", IsOnline = true });
        }

        [HttpGet("requestupdate")]
        public IActionResult RequestUpdate(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest("Missing device_id");

            deviceId = deviceId.ToLower();

            if (_connections.TryGetValue(deviceId, out var device))
            {
                device.RequestUpdate();
                return Ok("OK. Request for update sent.");
            }
            else
            {
                return BadRequest($"Device '{deviceId}' is not connected");
            }
        }

        [HttpGet("command")]
        public IActionResult Command(string deviceId, string command)
        {
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest("Missing device_id");
            if (string.IsNullOrEmpty(command))
                return BadRequest("Missing command");

            deviceId = deviceId.ToLower();

            if (_connections.TryGetValue(deviceId, out var device))
            {
                device.SendCommand(command);
                return Ok("OK. Request for command sent.");
            }
            else
            {
                return BadRequest($"Device '{deviceId}' is not connected");
            }
        }

        [HttpGet("status")]
        public IActionResult Status(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return BadRequest("Missing device_id");

            deviceId = deviceId.ToLower();

            if (_connections.ContainsKey(deviceId))
            {
                return Ok("Device is online");
            }
            else
            {
                return BadRequest($"Device '{deviceId}' is not connected");
            }
        }

        // Other existing endpoints...
        // Explicitly accepting HTTP methods
        [AcceptVerbs("GET", "POST")]
        [Route("{*url}", Order = int.MaxValue)]
        public IActionResult CatchAll()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key.ToLower(), h => h.Value.ToString());
            _logger.LogInformation("Got other request: " + Request.Path);

            if (headers.TryGetValue("x-esp8266-sta-mac", out var macAddress))
            {
                if (!Device.ValidateHeaders(headers))
                {
                    return BadRequest("Required headers are missing or invalid.");
                }
                _logger.LogInformation("> We have an ESP8266 update request", headers);

                if (_connections.TryGetValue(macAddress, out var device))
                {
                    // Additional logic based on your scenario...
                    return Ok($"Device {macAddress} is ready for an update.");
                }
                return NotFound("Device not found.");
            }

            return BadRequest("Unsupported device type or incorrect headers.");
        }
    }
}
