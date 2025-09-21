using Common.Models;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitoringServiceWorker
{
    public class HealthCheckService
    {

        private readonly HttpClient _httpClient;
        
        public HealthCheckService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        //CHECK THIS WHEN IMPLEMENT SOSERVICE 
        //public async Task<HealthCheck> CheckStackOverflowServiceAsync()
        //{

        //    var endpoints = RoleEnvironment.Roles["StackOverflowService"].Instances;
        //    var instance = endpoints.First();

        //    var endpoint = instance.InstanceEndpoints["HealthMonitoring"];
        //    var serviceUrl = $"http://{endpoint.IPEndpoint}";

        //    return await CheckServiceHealthAsync("StackOverflowService", serviceUrl);
        //}

        public async Task<HealthCheck> CheckNotificationServiceAsync()
        {
            var endpoints = RoleEnvironment.Roles["NotificationServiceWorker"].Instances;
            var instance = endpoints.First();
            var endpoint = instance.InstanceEndpoints["HealthMonitoring"];
            var serviceUrl = $"http://{endpoint.IPEndpoint}";

            return await CheckServiceHealthAsync("NotificationService", serviceUrl);
        }

        private async Task<HealthCheck> CheckServiceHealthAsync(string serviceName, string serviceUrl)
        {
            var healthCheck = new HealthCheck(serviceName);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _httpClient.GetAsync($"{serviceUrl}/health-monitoring");

                stopwatch.Stop();
                healthCheck.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    healthCheck.Status = "OK";
                    Trace.TraceInformation($"{serviceName} health check passed via internal endpoint");
                }
                else
                {
                    healthCheck.Status = "NOT_OK";
                    healthCheck.ErrorMessage = $"HTTP {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                healthCheck.Status = "NOT_OK";
                healthCheck.ErrorMessage = ex.Message;
                healthCheck.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                Trace.TraceError($"{serviceName} internal health check failed: {ex.Message}");
            }

            return healthCheck;
        }
    }
}
