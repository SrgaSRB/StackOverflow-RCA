using Azure;
using Azure.Data.Tables;
using System.Text.Json.Serialization;

namespace NotificationService.Models
{
    public class NotificationLog : ITableEntity
    {
        public string PartitionKey { get; set; } = "NOTIFICATION_LOG";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [JsonPropertyName("answerId")]
        public string AnswerId { get; set; } = string.Empty;

        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonPropertyName("emailsSent")]
        public int EmailsSent { get; set; }

        [JsonPropertyName("processedAt")]
        public DateTime ProcessedAt { get; set; }

        [JsonPropertyName("questionTitle")]
        public string QuestionTitle { get; set; } = string.Empty;

        [JsonPropertyName("bestAnswerAuthor")]
        public string BestAnswerAuthor { get; set; } = string.Empty;

        [JsonPropertyName("bestAnswerContent")]
        public string BestAnswerContent { get; set; } = string.Empty;
    }
}
