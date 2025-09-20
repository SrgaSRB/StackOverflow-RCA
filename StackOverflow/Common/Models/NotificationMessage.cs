using System;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class NotificationMessage
    {
        [JsonPropertyName("answerId")]
        public string AnswerId { get; set; } = string.Empty;

        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
