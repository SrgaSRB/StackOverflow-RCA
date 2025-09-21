using Microsoft.AspNetCore.Mvc;

namespace StackOverflow.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        public HealthController(){}

        [HttpGet("health-monitoring")]
        public IActionResult HealthCheck()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
