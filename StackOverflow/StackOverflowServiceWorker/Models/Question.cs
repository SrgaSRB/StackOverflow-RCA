namespace StackOverflow.Models
{
    public class Question : Entity
    {
        public Question()
        {
            PartitionKey = "QUESTION";
        }

        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? PictureUrl { get; set; }
        public required string UserId { get; set; }
        public string? BestCommentId { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;
    }
}