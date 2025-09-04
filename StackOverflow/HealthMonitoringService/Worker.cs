using HealthMonitoringService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthMonitoringService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HealthCheckService _healthCheckService;
        private readonly DistributedLockService _distributedLockService;
        private readonly IConfiguration _configuration;

        public Worker(
            ILogger<Worker> logger, 
            HealthCheckService healthCheckService, 
            DistributedLockService distributedLockService,
            IConfiguration configuration)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
            _distributedLockService = distributedLockService;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Health Monitoring Service starting... Instance: {InstanceId}", 
                _distributedLockService.GetInstanceId());
            
            // Initialize alert emails on startup
            await _healthCheckService.InitializeAlertEmailsAsync();
            
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Monitoring Service started. Instance: {InstanceId}. Checking services every 4 seconds.",
                _distributedLockService.GetInstanceId());

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Try to acquire distributed lock for 6 seconds (longer than 4s cycle)
                    const string lockName = "health-check-cycle";
                    var lockAcquired = await _distributedLockService.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(6));
                    
                    if (lockAcquired)
                    {
                        _logger.LogDebug("Instance {InstanceId} acquired lock - performing health checks", 
                            _distributedLockService.GetInstanceId());
                        
                        try
                        {
                            var services = new Dictionary<string, string>
                            {
                                { "StackOverflowService", _configuration["Services:StackOverflowService"]! },
                                { "NotificationService", _configuration["Services:NotificationService"]! }
                            };

                            var tasks = services.Select(async service =>
                            {
                                await _healthCheckService.CheckServiceHealthAsync(service.Key, service.Value);
                            });

                            await Task.WhenAll(tasks);

                            _logger.LogDebug("Instance {InstanceId} completed health checks for all services", 
                                _distributedLockService.GetInstanceId());
                        }
                        finally
                        {
                            // Always release lock
                            await _distributedLockService.ReleaseLockAsync(lockName);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Instance {InstanceId} - another instance is performing health checks", 
                            _distributedLockService.GetInstanceId());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check cycle for instance {InstanceId}", 
                        _distributedLockService.GetInstanceId());
                }

                // Wait 4 seconds before next check
                await Task.Delay(4000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Health Monitoring Service stopping... Instance: {InstanceId}", 
                _distributedLockService.GetInstanceId());
            await base.StopAsync(cancellationToken);
        }
    }
}
