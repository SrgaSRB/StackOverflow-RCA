namespace StackOverflow.Models
{
    public class Vote : Entity
    {
        public Vote()
        {
            PartitionKey = "VOTE";
        }

        public required string UserId { get; set; }
        public required string TargetId { get; set; } // QuestionId or CommentId
        public required string TargetType { get; set; } // "QUESTION" or "COMMENT"
        public required bool IsUpvote { get; set; } // true for upvote, false for downvote
    }
}
