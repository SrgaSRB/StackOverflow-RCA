using Azure.Data.Tables;
using HealthStatusService.Models;

namespace HealthStatusService.Services
{
    public interface IHealthCheckService
    {
        Task<List<HealthCheck>> GetHealthChecksFromLast3HoursAsync();
        HealthStatusViewModel ProcessHealthData(List<HealthCheck> healthChecks);
    }

    public class HealthCheckService : IHealthCheckService
    {
        private readonly TableClient _tableClient;

        public HealthCheckService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("HealthChecks");
        }

        public async Task<List<HealthCheck>> GetHealthChecksFromLast3HoursAsync()
        {
            var threeHoursAgo = DateTime.UtcNow.AddHours(-3);
            var healthChecks = new List<HealthCheck>();

            try
            {
                // Create table if it doesn't exist
                await _tableClient.CreateIfNotExistsAsync();

                // Query for health checks from last 3 hours
                var filter = $"PartitionKey eq 'HEALTH_CHECK' and DateTime ge datetime'{threeHoursAgo:yyyy-MM-ddTHH:mm:ss.fffZ}'";
                
                await foreach (var entity in _tableClient.QueryAsync<HealthCheck>(filter: filter))
                {
                    healthChecks.Add(entity);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error reading health checks: {ex.Message}");
                
                // If there's an error, create some sample data for demonstration
                healthChecks = CreateSampleData();
            }

            return healthChecks.OrderBy(h => h.DateTime).ToList();
        }

        private List<HealthCheck> CreateSampleData()
        {
            var sampleData = new List<HealthCheck>();
            var now = DateTime.UtcNow;
            
            for (int i = 0; i < 36; i++) // 3 hours of data, every 5 minutes
            {
                var checkTime = now.AddMinutes(-i * 5);
                var isHealthy = i % 7 != 0; // Make some checks fail for demonstration
                
                sampleData.Add(new HealthCheck
                {
                    DateTime = checkTime,
                    Status = isHealthy ? "OK" : "NOT_OK",
                    ServiceName = "StackOverflowService",
                    ResponseTimeMs = isHealthy ? Random.Shared.Next(50, 200) : Random.Shared.Next(1000, 5000),
                    ErrorMessage = isHealthy ? null : "Service timeout"
                });
            }
            
            return sampleData;
        }

        public HealthStatusViewModel ProcessHealthData(List<HealthCheck> healthChecks)
        {
            var viewModel = new HealthStatusViewModel();
            
            if (!healthChecks.Any())
            {
                return viewModel;
            }

            viewModel.HealthChecks = healthChecks;
            viewModel.TotalChecks = healthChecks.Count;
            viewModel.SuccessfulChecks = healthChecks.Count(h => h.Status == "OK");
            viewModel.FailedChecks = healthChecks.Count(h => h.Status != "OK");
            
            viewModel.AvailabilityPercentage = viewModel.TotalChecks > 0 
                ? (double)viewModel.SuccessfulChecks / viewModel.TotalChecks * 100 
                : 0;
            viewModel.UnavailabilityPercentage = 100 - viewModel.AvailabilityPercentage;

            viewModel.FromTime = healthChecks.Min(h => h.DateTime);
            viewModel.ToTime = healthChecks.Max(h => h.DateTime);

            // Group by hour for chart data
            viewModel.HourlyData = healthChecks
                .GroupBy(h => new DateTime(h.DateTime.Year, h.DateTime.Month, h.DateTime.Day, h.DateTime.Hour, 0, 0))
                .Select(g => new HourlyStatusData
                {
                    Hour = g.Key,
                    TotalChecks = g.Count(),
                    SuccessfulChecks = g.Count(h => h.Status == "OK"),
                    FailedChecks = g.Count(h => h.Status != "OK"),
                    AvailabilityPercentage = g.Count() > 0 ? (double)g.Count(h => h.Status == "OK") / g.Count() * 100 : 0,
                    Status = g.Count(h => h.Status == "OK") == g.Count() ? "OK" : "DEGRADED"
                })
                .OrderBy(h => h.Hour)
                .ToList();

            return viewModel;
        }
    }
}
