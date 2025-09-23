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
                int rand = new Random().Next(1, 5);
                if(rand ==1) 
                    throw new Exception("Simulated failure for health check");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
