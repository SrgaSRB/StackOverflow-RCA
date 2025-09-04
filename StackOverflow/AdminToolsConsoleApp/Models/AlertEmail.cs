using Azure;
using Azure.Data.Tables;

namespace AdminToolsConsoleApp.Models
{
    public class AlertEmail : ITableEntity
    {
        public string PartitionKey { get; set; } = "ALERT_EMAIL";
        public string RowKey { get; set; } = string.Empty; // Email adresa
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
