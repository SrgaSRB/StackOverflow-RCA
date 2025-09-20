namespace Common.Models
{
    public class Vote : Entity
    {
        public Vote()
        {
            PartitionKey = "VOTE";
        }

        public  string UserId { get; set; }
        public  string TargetId { get; set; } // QuestionId or CommentId
        public  string TargetType { get; set; } // "QUESTION" or "COMMENT"
        public  bool IsUpvote { get; set; } // true for upvote, false for downvote
    }
}
