namespace StackOverflow.Models
{
    public class Comment : Entity
    {
        public Comment()
        {
            PartitionKey = "COMMENT";
        }

        public required string Text { get; set; }
        public int Upvote { get; set; } = 0;
        public int Downvote { get; set; } = 0;
        public required string UserId { get; set; }
        public required string QuestionId { get; set; }

        public int TotalVotes => Upvote - Downvote;
    }
}