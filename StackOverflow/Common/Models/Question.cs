namespace Common.Models
{
    public class Question : Entity
    {
        public Question()
        {
            PartitionKey = "QUESTION";
        }

        public  string Title { get; set; }
        public  string Description { get; set; }
        public string PictureUrl { get; set; }
        public  string UserId { get; set; }
        public string BestCommentId { get; set; }
        public int Upvotes { get; set; } = 0;
        public int Downvotes { get; set; } = 0;
        public int TotalVotes { get; set; } = 0;
    }
}