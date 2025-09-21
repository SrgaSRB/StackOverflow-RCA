using System;
using System.Web.Http;

namespace StackOverflow.Controllers
{
    [RoutePrefix("")]
    public class HealthController : ApiController
    {
        public HealthController(){}

        [HttpGet, Route("health-monitoring")]
        public IHttpActionResult HealthCheck()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
