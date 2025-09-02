using Azure.Data.Tables;
using StackOverflow.Models;

namespace StackOverflow.Services
{
    public class CommentService
    {
        private readonly TableClient _tableClient;
        private readonly UserService _userService;

        public CommentService(string connectionString)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("Comments");
            _tableClient.CreateIfNotExists();
            _userService = new UserService(connectionString);
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

            await foreach (var comment in query)
            {
                var user = await _userService.GetUserAsync(comment.UserId);
                comments.Add(new
                {
                    AnswerId = comment.RowKey,
                    Content = comment.Text,
                    CreatedAt = comment.Timestamp,
                    TotalVotes = comment.TotalVotes,
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
    }
}
