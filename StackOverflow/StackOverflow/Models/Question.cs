namespace StackOverflow.Models
{
    public class Question : Entity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? PictureUrl { get; set; }
        public int Upvote { get; set; } = 0;
        public int Downvote { get; set; } = 0;
        public required string UserId { get; set; }
        public string? BestCommentId { get; set; }

        public int TotalVotes => Upvote - Downvote;
    }
}