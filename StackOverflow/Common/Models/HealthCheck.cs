using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class HealthCheck : TableEntity
    {
        public HealthCheck()
        {
            PartitionKey = "HEALTH_CHECK";
            RowKey = Guid.NewGuid().ToString();
        }

        public HealthCheck(string serviceName) : this()
        {
            ServiceName = serviceName;
            CheckDateTime = DateTime.UtcNow;
        }

        public DateTime CheckDateTime { get; set; }
        public string Status { get; set; } = ""; // "OK" ili "NOT_OK"
        public string ServiceName { get; set; } = ""; // "StackOverflowService" ili "NotificationService"
        public string ErrorMessage { get; set; } = "";
        public int ResponseTimeMs { get; set; }
    }
}
