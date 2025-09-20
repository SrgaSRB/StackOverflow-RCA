using Azure;
using Azure.Data.Tables;
using StackOverflow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackOverflow.Services
{
    public class CommentService
    {
        private readonly TableClient _tableClient;
        private readonly UserService _userService;
        private readonly VoteService _voteService;

        public CommentService(string connectionString, UserService userService, VoteService voteService)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("Comments");
            _tableClient.CreateIfNotExists();
            _userService = userService;
            _voteService = voteService;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _tableClient.AddEntityAsync(comment);
            return comment;
        }

        public async Task<IEnumerable<object>> GetCommentsForQuestionAsync(string questionId)
        {
            var comments = new List<object>();
            var query = _tableClient.QueryAsync<Comment>(c => c.QuestionId == questionId);

            foreach (var comment in query)
            {
                var user = await _userService.GetUserAsync(comment.UserId);
                
                // Get vote stats (use cached values from entity if available)
                var upvotes = comment.Upvotes;
                var downvotes = comment.Downvotes;
                var totalVotes = comment.TotalVotes;
                
                // If vote counts are all zero, fall back to calculating from Vote table (for existing data)
                if (upvotes == 0 && downvotes == 0 && totalVotes == 0)
                {
                    (upvotes, downvotes, totalVotes) = await _voteService.GetVoteStatsAsync(comment.RowKey, "COMMENT");
                }
                
                comments.Add(new
                {
                    AnswerId = comment.RowKey,
                    Content = comment.Text,
                    CreatedAt = comment.Timestamp,
                    Upvotes = upvotes,
                    Downvotes = downvotes,
                    TotalVotes = totalVotes,
                    User = new
                    {
                        Username = user?.Username,
                        ProfilePictureUrl = user?.ProfilePictureUrl,
                        QuestionsCount = 0 // This needs a proper implementation if required
                    }
                });
            }
            return comments;
        }

        public async Task<Comment?> GetCommentByIdAsync(string commentId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Comment>("COMMENT", commentId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> UpvoteCommentAsync(string commentId, string userId)
        {
            return await _voteService.VoteAsync(userId, commentId, "COMMENT", true);
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> DownvoteCommentAsync(string commentId, string userId)
        {
            return await _voteService.VoteAsync(userId, commentId, "COMMENT", false);
        }

        public async Task<string?> GetUserVoteAsync(string userId, string commentId)
        {
            return await _voteService.GetUserVoteTypeAsync(userId, commentId, "COMMENT");
        }

        public async Task<int> GetUserAnswersCountAsync(string userId)
        {
            int count = 0;
            await foreach (var comment in _tableClient.QueryAsync<Comment>(c => c.UserId == userId))
            {
                count++;
            }
            return count;
        }
    }
}
