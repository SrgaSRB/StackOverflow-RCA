using Microsoft.AspNetCore.Mvc;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet("health-monitoring")]
        public IActionResult HealthCheck()
        {
            try
            {
                // Perform basic health checks here
                // For example: check database connectivity, external dependencies, etc.
                
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "StackOverflowService",
                    Version = "1.0.0"
                };

                _logger.LogInformation("Health check requested - Status: Healthy");
                
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                var errorStatus = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "StackOverflowService",
                    Error = ex.Message
                };
                
                return StatusCode(500, errorStatus);
            }
        }
    }
}
