using HealthStatusServiceWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace HealthStatusServiceWeb.Controllers
{
    [Route("api/health-checks")]
    public class HealthCheckController : ApiController
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> GetServicesStatus()
        {
            var result = await _healthCheckService.GetServicesStatusLast3Hours();
            return Ok(result);
        }

    }
}