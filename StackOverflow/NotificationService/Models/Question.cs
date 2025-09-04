using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace NotificationService.Models
{
    public class Question : ITableEntity
    {
        public string PartitionKey { get; set; } = "QUESTION";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("pictureUrl")]
        public string? PictureUrl { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("upvotes")]
        public int Upvotes { get; set; } = 0;

        [JsonPropertyName("downvotes")]
        public int Downvotes { get; set; } = 0;

        [JsonPropertyName("totalVotes")]
        public int TotalVotes { get; set; } = 0;

        [JsonPropertyName("bestCommentId")]
        public string? BestCommentId { get; set; }
    }
}
