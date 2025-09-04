using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace NotificationService.Models
{
    public class Comment : ITableEntity
    {
        public string PartitionKey { get; set; } = "COMMENT";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonPropertyName("upvotes")]
        public int Upvotes { get; set; } = 0;

        [JsonPropertyName("downvotes")]
        public int Downvotes { get; set; } = 0;

        [JsonPropertyName("totalVotes")]
        public int TotalVotes { get; set; } = 0;
    }
}
