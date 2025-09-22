using Azure.Data.Tables;
using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class CommentService
    {
        private readonly CloudTable _commentsTable;
        private readonly UserService _userService;
        private readonly VoteService _voteService;

        public CommentService(string connectionString, UserService userService, VoteService voteService)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            _commentsTable = tableClient.GetTableReference("Comments");
            _commentsTable.CreateIfNotExists();

            _userService = userService;
            _voteService = voteService;
        }

        public static CommentService FromConfig(UserService userService, VoteService voteService)
        {
            var cs = CloudConfigurationManager.GetSetting("StorageConnectionString");
            return new CommentService(cs, userService, voteService);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            var insert = TableOperation.Insert(comment);
            await _commentsTable.ExecuteAsync(insert);
            return comment;
        }

        public async Task<IEnumerable<object>> GetCommentsForQuestionAsync(string questionId)
        {
            var results = new List<object>();

            var pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "COMMENT");
            var qidFilter = TableQuery.GenerateFilterCondition("QuestionId", QueryComparisons.Equal, questionId);
            var filter = TableQuery.CombineFilters(pkFilter, TableOperators.And, qidFilter);

            var query = new TableQuery<Comment>().Where(filter);

            TableContinuationToken token = null;
            do
            {
                var segment = await _commentsTable.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;

                foreach (var comment in segment.Results)
                {
                    var user = await _userService.GetUserAsync(comment.UserId);

                    var upvotes = comment.Upvotes;
                    var downvotes = comment.Downvotes;
                    var totalVotes = comment.TotalVotes;

                    if (upvotes == 0 && downvotes == 0 && totalVotes == 0)
                    {
                        (upvotes, downvotes, totalVotes) = await _voteService.GetVoteStatsAsync(comment.RowKey, "COMMENT");
                    }

                    results.Add(new
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
                            QuestionsCount = 0 
                        }
                    });
                }
            } while (token != null);

            return results;
        }

        public async Task<Comment> GetCommentByIdAsync(string commentId)
        {
            var retrieve = TableOperation.Retrieve<Comment>("COMMENT", commentId);
            var res = await _commentsTable.ExecuteAsync(retrieve);

            return res.Result as Comment; 
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> UpvoteCommentAsync(string commentId, string userId)
        {
            return await _voteService.VoteAsync(userId, commentId, "COMMENT", true);
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> DownvoteCommentAsync(string commentId, string userId)
        {
            return await _voteService.VoteAsync(userId, commentId, "COMMENT", false);
        }

        public async Task<string> GetUserVoteAsync(string userId, string commentId)
        {
            return await _voteService.GetUserVoteTypeAsync(userId, commentId, "COMMENT");
        }

        public async Task<int> GetUserAnswersCountAsync(string userId)
        {
            var pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "COMMENT");
            var uidFilter = TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId);
            var filter = TableQuery.CombineFilters(pkFilter, TableOperators.And, uidFilter);

            var query = new TableQuery<Comment>().Where(filter);

            int count = 0;
            TableContinuationToken token = null;
            do
            {
                var segment = await _commentsTable.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                count += segment.Results.Count;
            } while (token != null);

            return count;
        }
    }
}