using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflow.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class QuestionService
    {
        private readonly CloudTable _questionsTable;
        private readonly CloudTable _usersTable;
        private readonly CloudTable _commentsTable;
        private readonly CloudBlobContainer _picturesContainer;
        private readonly VoteService _voteService;

        public QuestionService(string connectionString, VoteService voteService)
        {
            var account = CloudStorageAccount.Parse(connectionString);

            var tableClient = account.CreateCloudTableClient();
            _questionsTable = tableClient.GetTableReference("Questions");
            _usersTable = tableClient.GetTableReference("Users");
            _commentsTable = tableClient.GetTableReference("Comments");

            _questionsTable.CreateIfNotExists();
            _usersTable.CreateIfNotExists();
            _commentsTable.CreateIfNotExists();

            var blobClient = account.CreateCloudBlobClient();
            _picturesContainer = blobClient.GetContainerReference("question-pictures");
            _picturesContainer.CreateIfNotExists();

            _voteService = voteService;

            _picturesContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public async Task<List<QuestionDetails>> GetAllQuestionsWithUserDetailsAsync()
        {
            var list = new List<Question>();

            await ForEachSegmentAsync(_questionsTable, new TableQuery<Question>(), seg =>
            {
                list.AddRange(seg.Results);
                return Task.CompletedTask;
            });

            list = list.OrderByDescending(q => q.CreatedDate).ToList();

            var result = new List<QuestionDetails>();

            foreach (var q in list)
            {
                User user = null;
                try
                {
                    var getUser = TableOperation.Retrieve<User>("USER", q.UserId);
                    var res = await _usersTable.ExecuteAsync(getUser);
                    user = res.Result as User;
                }
                catch { /* ignore */ }

                var up = q.Upvotes; var down = q.Downvotes; var tot = q.TotalVotes;
                if (up == 0 && down == 0 && tot == 0)
                {
                    (up, down, tot) = await _voteService.GetVoteStatsAsync(q.RowKey, "QUESTION");
                }

                int answersCount = await CountCommentsForQuestionAsync(q.RowKey);

                result.Add(new QuestionDetails
                {
                    QuestionId = q.RowKey,
                    Title = q.Title,
                    Description = q.Description,
                    PictureUrl = q.PictureUrl,
                    Upvotes = up,
                    Downvotes = down,
                    TotalVotes = tot,
                    CreatedAt = q.Timestamp.UtcDateTime, 
                    User = user != null
                        ? new UserInfo { Username = user.Username, ProfilePictureUrl = user.ProfilePictureUrl, QuestionsCount = user.QuestionsCount }
                        : new UserInfo { Username = "Unknown" },
                    AnswersCount = answersCount
                });
            }

            return result;
        }

        public async Task<Question> CreateQuestionAsync(Question q)
        {
            var insert = TableOperation.Insert(q);
            await _questionsTable.ExecuteAsync(insert);
            return q;
        }

        public async Task UpdateQuestionAsync(Question q)
        {
            var replace = TableOperation.Replace(q);
            await _questionsTable.ExecuteAsync(replace);
        }

        public async Task<string> UploadQuestionPictureAsync(Stream fileStream, string originalFileName)
        {
            var name = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            var blob = _picturesContainer.GetBlockBlobReference(name);
            await blob.UploadFromStreamAsync(fileStream);
            return blob.Uri.ToString();
        }

        public async Task DeletePictureAsync(string pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl)) return;

            try
            {
                var uri = new Uri(pictureUrl);
                var name = Path.GetFileName(uri.LocalPath);
                var blob = _picturesContainer.GetBlockBlobReference(name);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"DeletePicture failed: {ex.Message}");
            }
        }

        public async Task<List<Question>> GetQuestionsByUserIdAsync(string userId)
        {
            var res = new List<Question>();

            var f1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "QUESTION");
            var f2 = TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId);
            var filter = TableQuery.CombineFilters(f1, TableOperators.And, f2);

            var query = new TableQuery<Question>().Where(filter);

            await ForEachSegmentAsync(_questionsTable, query, seg =>
            {
                res.AddRange(seg.Results);
                return Task.CompletedTask;
            });

            return res;
        }

        public async Task<List<QuestionDetails>> GetQuestionsByUserIdWithDetailsAsync(string userId)
        {
            var qs = await GetQuestionsByUserIdAsync(userId);
            qs = qs.OrderByDescending(q => q.CreatedDate).ToList();

            var list = new List<QuestionDetails>();
            foreach (var q in qs)
            {
                User user = null;
                try
                {
                    var getUser = TableOperation.Retrieve<User>("USER", q.UserId);
                    var res = await _usersTable.ExecuteAsync(getUser);
                    user = res.Result as User;
                }
                catch { }

                var up = q.Upvotes; var down = q.Downvotes; var tot = q.TotalVotes;
                if (up == 0 && down == 0 && tot == 0)
                {
                    (up, down, tot) = await _voteService.GetVoteStatsAsync(q.RowKey, "QUESTION");
                }

                int answersCount = await CountCommentsForQuestionAsync(q.RowKey);

                list.Add(new QuestionDetails
                {
                    QuestionId = q.RowKey,
                    Title = q.Title,
                    Description = q.Description,
                    PictureUrl = q.PictureUrl,
                    Upvotes = up,
                    Downvotes = down,
                    TotalVotes = tot,
                    CreatedAt = q.Timestamp.UtcDateTime,
                    User = user != null
                        ? new UserInfo { Username = user.Username, ProfilePictureUrl = user.ProfilePictureUrl, QuestionsCount = user.QuestionsCount }
                        : new UserInfo { Username = "Unknown" },
                    AnswersCount = answersCount
                });
            }

            return list;
        }

        public async Task<Question> GetQuestionByIdAsync(string questionId)
        {
            var get = TableOperation.Retrieve<Question>("QUESTION", questionId);
            var res = await _questionsTable.ExecuteAsync(get);
            return res.Result as Question; // može biti null
        }

        public async Task<QuestionDetails> GetQuestionWithDetailsAsync(string questionId)
        {
            var q = await GetQuestionByIdAsync(questionId);
            if (q == null) return null;

            User user = null;
            int actualCount = 0;
            try
            {
                var getUser = TableOperation.Retrieve<User>("USER", q.UserId);
                var resUser = await _usersTable.ExecuteAsync(getUser);
                user = resUser.Result as User;

                var f1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "QUESTION");
                var f2 = TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, q.UserId);
                var qFilter = TableQuery.CombineFilters(f1, TableOperators.And, f2);

                await ForEachSegmentAsync(_questionsTable, new TableQuery<Question>().Where(qFilter), seg =>
                {
                    actualCount += seg.Results.Count;
                    return Task.CompletedTask;
                });
            }
            catch { }

            var up = q.Upvotes; var down = q.Downvotes; var tot = q.TotalVotes;
            if (up == 0 && down == 0 && tot == 0)
            {
                (up, down, tot) = await _voteService.GetVoteStatsAsync(q.RowKey, "QUESTION");
            }

            return new QuestionDetails
            {
                QuestionId = q.RowKey,
                Title = q.Title,
                Description = q.Description,
                PictureUrl = q.PictureUrl,
                Upvotes = up,
                Downvotes = down,
                TotalVotes = tot,
                CreatedAt = q.Timestamp.UtcDateTime,
                User = user != null
                    ? new UserInfo { Username = user.Username, ProfilePictureUrl = user.ProfilePictureUrl, QuestionsCount = actualCount }
                    : new UserInfo { Username = "Unknown" },
                Answers = new List<Answer>() 
            };
        }

        public async Task DeleteQuestionAsync(string questionId)
        {
            var q = await GetQuestionByIdAsync(questionId);
            if (q == null) return;

            // 1) obriši sliku ako postoji
            if (!string.IsNullOrWhiteSpace(q.PictureUrl))
            {
                try
                {
                    var uri = new Uri(q.PictureUrl);
                    var name = Path.GetFileName(uri.LocalPath);
                    var blob = _picturesContainer.GetBlockBlobReference(name);
                    await blob.DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"Delete picture failed: {ex.Message}");
                }
            }

            var del = TableOperation.Delete(q);
            await _questionsTable.ExecuteAsync(del);
        }

        public Task<(int upvotes, int downvotes, int totalVotes)> UpvoteQuestionAsync(string questionId, string userId)
            => _voteService.VoteAsync(userId, questionId, "QUESTION", true);

        public Task<(int upvotes, int downvotes, int totalVotes)> DownvoteQuestionAsync(string questionId, string userId)
            => _voteService.VoteAsync(userId, questionId, "QUESTION", false);

        public Task<string> GetUserVoteAsync(string userId, string questionId)
            => _voteService.GetUserVoteTypeAsync(userId, questionId, "QUESTION");

        public async Task<List<QuestionDetails>> GetPopularQuestionsAsync(int limit = 5)
        {
            var all = await GetAllQuestionsWithUserDetailsAsync();
            var sorted = all.OrderByDescending(q => q.TotalVotes)
                            .ThenByDescending(q => q.CreatedAt)
                            .ToList();

            return (limit >= 100) ? sorted : sorted.Take(limit).ToList();
        }

        public async Task MarkBestAnswerAsync(string questionId, string answerId)
        {
            var q = await GetQuestionByIdAsync(questionId);
            if (q == null) return;

            q.BestCommentId = answerId;
            await _questionsTable.ExecuteAsync(TableOperation.Replace(q));
        }

        public async Task UnmarkBestAnswerAsync(string questionId)
        {
            var q = await GetQuestionByIdAsync(questionId);
            if (q == null) return;

            q.BestCommentId = null;
            await _questionsTable.ExecuteAsync(TableOperation.Replace(q));
        }


        private static async Task ForEachSegmentAsync<T>(CloudTable table, TableQuery<T> query, Func<TableQuerySegment<T>, Task> onSegment)
            where T : Microsoft.WindowsAzure.Storage.Table.ITableEntity, new()
        {
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                await onSegment(seg);
            } while (token != null);
        }

        private async Task<int> CountCommentsForQuestionAsync(string questionId)
        {
            int count = 0;

            var f1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "COMMENT");
            var f2 = TableQuery.GenerateFilterCondition("QuestionId", QueryComparisons.Equal, questionId);
            var filter = TableQuery.CombineFilters(f1, TableOperators.And, f2);

            var query = new TableQuery<Comment>().Where(filter);

            await ForEachSegmentAsync(_commentsTable, query, seg =>
            {
                count += seg.Results.Count;
                return Task.CompletedTask;
            });

            return count;
        }
    }
}