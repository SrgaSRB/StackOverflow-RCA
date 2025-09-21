using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    public class HealthServer
    {
        private ServiceHost _serviceHost;

        public void Start()
        {
            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HealthMonitoring"];
            var uri = $"http://{endpoint.IPEndpoint}/";

            _serviceHost = new ServiceHost(typeof(HealthMonitoringServer));

            var binding = new WebHttpBinding();
            var serviceEndpoint = _serviceHost.AddServiceEndpoint(
                typeof(IHealthMonitoring),
                binding,
                uri
            );
            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());

            _serviceHost.Open();
            Trace.TraceInformation($"Health monitoring endpoint started at: {uri}");
        }

        public void Stop()
        {
            _serviceHost?.Close();
            Trace.TraceInformation("Health monitoring endpoint stopped.");
        }


    }
}
