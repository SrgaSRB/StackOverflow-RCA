using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace StackOverflow.Models
{
    public class User : ITableEntity
    {
        // Azure Table Storage required properties
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        
        public DateTimeOffset? Timestamp { get; set; }
        
        [JsonIgnore] // Don't serialize ETag to avoid conflicts
        public ETag ETag { get; set; }

        // User properties
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Country { get; set; } = "";
        public string City { get; set; } = "";
        public string StreetAddress { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }
        public bool? IsAdmin { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
        public int QuestionsCount { get; set; } = 0;
        public int AnswersCount { get; set; } = 0;
    }
}