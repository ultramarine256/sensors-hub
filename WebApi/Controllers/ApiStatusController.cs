using Microsoft.AspNetCore.Mvc;

namespace SensorsHub.Controllers
{
    [ApiController]
    [Route("/")]
    public class ApiStatusController : ControllerBase
    {
        private readonly ILogger<ApiStatusController> _logger;
        private readonly IConfiguration _configuration;

        public ApiStatusController(ILogger<ApiStatusController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                name = "SensorsHub API",
                version = "1.0.0",
                buildCounter = "build:counter", //_configuration["build:counter"],
                deployDateTimeUTC = "deploy:datetime" // _configuration["deploy:datetime"]
            });
        }
    }
}
