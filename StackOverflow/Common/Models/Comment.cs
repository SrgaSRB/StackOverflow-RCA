namespace Common.Models
{
    public class Comment : Entity
    {
        public Comment()
        {
            PartitionKey = "COMMENT";
        }

        public string Text { get; set; }
        public string UserId { get; set; }
        public string QuestionId { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;
    }
}