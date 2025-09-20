using Azure;
using Azure.Data.Tables;

namespace StackOverflow.Models
{
    public abstract class Entity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Id 
        { 
            get => RowKey; 
            set => RowKey = value; 
        }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        protected Entity()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}