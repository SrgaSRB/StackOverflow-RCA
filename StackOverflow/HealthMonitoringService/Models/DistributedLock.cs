using Azure;
using Azure.Data.Tables;

namespace HealthMonitoringService.Models
{
    public class DistributedLock : ITableEntity
    {
        public string PartitionKey { get; set; } = "LOCK";
        public string RowKey { get; set; } = string.Empty; // Lock name
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string InstanceId { get; set; } = string.Empty;
        public DateTime AcquiredAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
