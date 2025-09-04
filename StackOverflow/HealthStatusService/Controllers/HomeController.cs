using Microsoft.AspNetCore.Mvc;
using HealthStatusService.Services;

namespace HealthStatusService.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHealthCheckService _healthCheckService;

        public HomeController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async Task<IActionResult> Index()
        {
            var healthChecks = await _healthCheckService.GetHealthChecksFromLast3HoursAsync();
            var viewModel = _healthCheckService.ProcessHealthData(healthChecks);
            
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            var healthChecks = await _healthCheckService.GetHealthChecksFromLast3HoursAsync();
            var viewModel = _healthCheckService.ProcessHealthData(healthChecks);
            
            var chartData = new
            {
                labels = viewModel.HourlyData.Select(h => h.Hour.ToString("HH:mm")).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Availability %",
                        data = viewModel.HourlyData.Select(h => h.AvailabilityPercentage).ToArray(),
                        backgroundColor = "rgba(75, 192, 192, 0.2)",
                        borderColor = "rgba(75, 192, 192, 1)",
                        borderWidth = 1
                    }
                }
            };

            return Json(chartData);
        }
    }
}
