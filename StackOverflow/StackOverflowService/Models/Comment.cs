namespace StackOverflow.Models
{
    public class Comment : Entity
    {
        public Comment()
        {
            PartitionKey = "COMMENT";
        }

        public required string Text { get; set; }
        public required string UserId { get; set; }
        public required string QuestionId { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;
    }
}