using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    public class HealthMonitoringServer : IHealthMonitoring
    {
        public string HealthCheck()
        {
            return "OK";
        }
    }
}
