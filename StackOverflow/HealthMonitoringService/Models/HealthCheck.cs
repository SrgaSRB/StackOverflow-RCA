using Azure;
using Azure.Data.Tables;

namespace HealthMonitoringService.Models
{
    public class HealthCheck : ITableEntity
    {
        public string PartitionKey { get; set; } = "HEALTH_CHECK";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public DateTime DateTime { get; set; }
        public string Status { get; set; } = string.Empty; // "OK" ili "NOT_OK"
        public string ServiceName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int ResponseTimeMs { get; set; }
    }
}
