using Azure;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Common.Models
{
    public abstract class Entity : TableEntity
    {
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