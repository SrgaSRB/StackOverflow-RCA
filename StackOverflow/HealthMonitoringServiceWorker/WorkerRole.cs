using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoringServiceWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private DataService _dataService;
        private HealthCheckService _healthCheckService;
        private EmailAlertService _emailAlertService;

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringServiceWorker is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            _dataService = new DataService();
            _emailAlertService = new EmailAlertService();
            _healthCheckService = new HealthCheckService();

            bool result = base.OnStart();

            Trace.TraceInformation("HealthMonitoringServiceWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringServiceWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("HealthMonitoringServiceWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {

                try
                {
                    //var soHealth = await _healthCheckService.CheckStackOverflowServiceAsync();
                    //await _dataService.SaveHealthCheckAsync(soHealth);

                    //if (soHealth.Status != "OK")
                    //{
                    //    var alertEmails = await _dataService.GetActiveEmailsAsync();
                    //    if (alertEmails.Count > 0)
                    //    {
                    //        await _emailAlertService.SendHealthAlertAsync(alertEmails, soHealth);
                    //    }
                    //}

                    var notificationHealth = await _healthCheckService.CheckNotificationServiceAsync();
                    await _dataService.SaveHealthCheckAsync(notificationHealth);

                    if (notificationHealth.Status != "OK")
                    {
                        var alertEmails = await _dataService.GetActiveEmailsAsync();
                        if (alertEmails.Count > 0)
                        {
                            await _emailAlertService.SendHealthAlertAsync(alertEmails, notificationHealth);
                        }
                    }

                    Trace.TraceInformation($"Health checks completed Notification: {notificationHealth.Status}");
                    //Trace.TraceInformation($"Health checks completed - StackOverflow: {soHealth.Status}, Notification: {notificationHealth.Status}");

                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error during health checks: {ex.Message}");
                }

                await Task.Delay(4000);
            }
        }
    }
}
