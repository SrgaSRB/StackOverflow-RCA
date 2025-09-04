using Azure.Data.Tables;
using HealthMonitoringService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HealthMonitoringService.Services
{
    public class HealthCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly TableClient _healthCheckTableClient;
        private readonly TableClient _alertEmailTableClient;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            EmailService emailService,
            ILogger<HealthCheckService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;

            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                                  "UseDevelopmentStorage=true";

            _healthCheckTableClient = new TableClient(connectionString, "HealthCheck");
            _healthCheckTableClient.CreateIfNotExists();

            _alertEmailTableClient = new TableClient(connectionString, "AlertEmails");
            _alertEmailTableClient.CreateIfNotExists();

            // Set HTTP client timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task CheckServiceHealthAsync(string serviceName, string serviceUrl)
        {
            var stopwatch = Stopwatch.StartNew();
            var healthCheck = new HealthCheck
            {
                DateTime = DateTime.UtcNow,
                ServiceName = serviceName
            };

            try
            {
                var response = await _httpClient.GetAsync($"{serviceUrl}/health-monitoring");
                stopwatch.Stop();
                
                healthCheck.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    healthCheck.Status = "OK";
                    _logger.LogInformation("Health check passed for {ServiceName} in {ResponseTime}ms", 
                        serviceName, healthCheck.ResponseTimeMs);
                }
                else
                {
                    healthCheck.Status = "NOT_OK";
                    healthCheck.ErrorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                    _logger.LogWarning("Health check failed for {ServiceName}: {StatusCode}", 
                        serviceName, response.StatusCode);
                    
                    await SendAlertEmailsAsync(serviceName, healthCheck.ErrorMessage);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                stopwatch.Stop();
                healthCheck.Status = "NOT_OK";
                healthCheck.ErrorMessage = "Request timeout";
                healthCheck.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                _logger.LogError("Health check timeout for {ServiceName}", serviceName);
                await SendAlertEmailsAsync(serviceName, "Request timeout");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                healthCheck.Status = "NOT_OK";
                healthCheck.ErrorMessage = ex.Message;
                healthCheck.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                
                _logger.LogError(ex, "Health check error for {ServiceName}", serviceName);
                await SendAlertEmailsAsync(serviceName, ex.Message);
            }

            // Save health check result to table
            await SaveHealthCheckAsync(healthCheck);
        }

        private async Task SaveHealthCheckAsync(HealthCheck healthCheck)
        {
            try
            {
                await _healthCheckTableClient.AddEntityAsync(healthCheck);
                _logger.LogInformation("Health check saved: {ServiceName} - {Status}", 
                    healthCheck.ServiceName, healthCheck.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving health check for {ServiceName}", healthCheck.ServiceName);
            }
        }

        private async Task SendAlertEmailsAsync(string serviceName, string errorMessage)
        {
            try
            {
                var alertEmails = new List<AlertEmail>();
                
                await foreach (var email in _alertEmailTableClient.QueryAsync<AlertEmail>(
                    e => e.IsActive == true))
                {
                    alertEmails.Add(email);
                }

                foreach (var alertEmail in alertEmails)
                {
                    await _emailService.SendAlertEmailAsync(
                        alertEmail.Email, 
                        alertEmail.Name, 
                        serviceName, 
                        errorMessage);
                }

                _logger.LogInformation("Sent {Count} alert emails for {ServiceName}", 
                    alertEmails.Count, serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert emails for {ServiceName}", serviceName);
            }
        }

        public async Task InitializeAlertEmailsAsync()
        {
            try
            {
                // Check if there are any existing alert emails
                var existingEmails = new List<AlertEmail>();
                await foreach (var email in _alertEmailTableClient.QueryAsync<AlertEmail>())
                {
                    existingEmails.Add(email);
                }

                if (!existingEmails.Any())
                {
                    // Add default alert emails
                    var defaultEmails = new[]
                    {
                        new AlertEmail { RowKey = "admin@stackoverflow.com", Email = "admin@stackoverflow.com", Name = "Admin", IsActive = true },
                        new AlertEmail { RowKey = "devops@stackoverflow.com", Email = "devops@stackoverflow.com", Name = "DevOps", IsActive = true }
                    };

                    foreach (var email in defaultEmails)
                    {
                        await _alertEmailTableClient.AddEntityAsync(email);
                    }

                    _logger.LogInformation("Initialized {Count} default alert emails", defaultEmails.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing alert emails");
            }
        }
    }
}
