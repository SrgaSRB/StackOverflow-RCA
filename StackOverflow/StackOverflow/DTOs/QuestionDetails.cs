using StackOverflow.Models;

namespace StackOverflow.DTOs
{
    public class QuestionDetails
    {
        public string QuestionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserInfo? User { get; set; }
        public int AnswersCount { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int QuestionsCount { get; set; }
    }

    public class Answer
    {
        public string AnswerId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
        public int TotalVotes { get; set; }
    }
}
