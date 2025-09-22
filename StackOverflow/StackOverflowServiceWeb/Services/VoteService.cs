using Microsoft.WindowsAzure.Storage.Table;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class VoteService
    {
        private readonly CloudTable _votesTable;
        private readonly CloudTable _questionsTable;
        private readonly CloudTable _commentsTable;

        public VoteService(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();

            _votesTable = tableClient.GetTableReference("Votes");
            _votesTable.CreateIfNotExists();

            _questionsTable = tableClient.GetTableReference("Questions");
            _questionsTable.CreateIfNotExists();

            _commentsTable = tableClient.GetTableReference("Comments");
            _commentsTable.CreateIfNotExists();
        }

        public async Task<Vote> GetUserVoteAsync(string userId, string targetId, string targetType)
        {
            try
            {
                // Query: UserId == userId AND TargetId == targetId AND TargetType == targetType
                var f1 = TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId);
                var f2 = TableQuery.GenerateFilterCondition("TargetId", QueryComparisons.Equal, targetId);
                var f3 = TableQuery.GenerateFilterCondition("TargetType", QueryComparisons.Equal, targetType);
                var filter = TableQuery.CombineFilters(TableQuery.CombineFilters(f1, TableOperators.And, f2), TableOperators.And, f3);

                var query = new TableQuery<Vote>().Where(filter);

                TableContinuationToken token = null;
                do
                {
                    var segment = await _votesTable.ExecuteQuerySegmentedAsync(query, token);
                    token = segment.ContinuationToken;
                    var hit = segment.Results.FirstOrDefault();
                    if (hit != null) return hit;
                } while (token != null);

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> GetVoteStatsAsync(string targetId, string targetType)
        {
            int up = 0, down = 0;

            var f1 = TableQuery.GenerateFilterCondition("TargetId", QueryComparisons.Equal, targetId);
            var f2 = TableQuery.GenerateFilterCondition("TargetType", QueryComparisons.Equal, targetType);
            var filter = TableQuery.CombineFilters(f1, TableOperators.And, f2);

            var query = new TableQuery<Vote>().Where(filter);

            TableContinuationToken token = null;
            do
            {
                var segment = await _votesTable.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;

                foreach (var v in segment.Results)
                {
                    if (v.IsUpvote) up++; else down++;
                }
            } while (token != null);

            return (up, down, up + down);
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> VoteAsync(string userId, string targetId, string targetType, bool isUpvote)
        {
            var existing = await GetUserVoteAsync(userId, targetId, targetType);

            if (existing != null)
            {
                if (existing.IsUpvote == isUpvote)
                {
                    var del = TableOperation.Delete(existing);
                    await _votesTable.ExecuteAsync(del);
                }
                else
                {
                    existing.IsUpvote = isUpvote;
                    var replace = TableOperation.Replace(existing);
                    await _votesTable.ExecuteAsync(replace);
                }
            }
            else
            {
                var vote = new Vote
                {
                    PartitionKey = string.IsNullOrEmpty(votePartitionFor(targetType, targetId)) ? "VOTE" : votePartitionFor(targetType, targetId),
                    RowKey = Guid.NewGuid().ToString(),

                    UserId = userId,
                    TargetId = targetId,
                    TargetType = targetType,
                    IsUpvote = isUpvote,
                    Timestamp = DateTimeOffset.UtcNow
                };

                var insert = TableOperation.Insert(vote);
                await _votesTable.ExecuteAsync(insert);
            }

            var (up, down, total) = await GetVoteStatsAsync(targetId, targetType);
            await UpdateTargetVoteCountsAsync(targetId, targetType, up, down, total);

            return (up, down, total);
        }

        private static string votePartitionFor(string targetType, string targetId)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (targetId == null)
                throw new ArgumentNullException(nameof(targetId));

            return null;
        }

        private async Task UpdateTargetVoteCountsAsync(string targetId, string targetType, int upvotes, int downvotes, int totalVotes)
        {
            try
            {
                if (string.Equals(targetType, "QUESTION", StringComparison.OrdinalIgnoreCase))
                {
                    var get = TableOperation.Retrieve<Question>("QUESTION", targetId);
                    var res = await _questionsTable.ExecuteAsync(get);
                    if (res.Result is Question q)
                    {
                        q.Upvotes = upvotes;
                        q.Downvotes = downvotes;
                        q.TotalVotes = totalVotes;

                        var replace = TableOperation.Replace(q);
                        await _questionsTable.ExecuteAsync(replace);
                    }
                }
                else if (string.Equals(targetType, "COMMENT", StringComparison.OrdinalIgnoreCase))
                {
                    var get = TableOperation.Retrieve<Comment>("COMMENT", targetId);
                    var res = await _commentsTable.ExecuteAsync(get);
                    if (res.Result is Comment c)
                    {
                        c.Upvotes = upvotes;
                        c.Downvotes = downvotes;
                        c.TotalVotes = totalVotes;

                        var replace = TableOperation.Replace(c);
                        await _commentsTable.ExecuteAsync(replace);
                    }
                }
            }
            catch (StorageException ex) when (ex.RequestInformation?.HttpStatusCode == 404)
            {
            }
            catch (Exception ex)
            {
                Trace.TraceError($"UpdateTargetVoteCountsAsync error: {ex.Message}");
            }
        }

        public async Task<string> GetUserVoteTypeAsync(string userId, string targetId, string targetType)
        {
            var v = await GetUserVoteAsync(userId, targetId, targetType);
            if (v == null) return null;
            return v.IsUpvote ? "upvote" : "downvote";
        }

        public async Task MigrateVoteCountsAsync()
        {
            // QUESTIONS
            await ForEachSegmentAsync(_questionsTable, new TableQuery<Question>(), async q =>
            {
                var (up, down, total) = await GetVoteStatsAsync(q.RowKey, "QUESTION");
                if (q.Upvotes != up || q.Downvotes != down || q.TotalVotes != total)
                {
                    q.Upvotes = up;
                    q.Downvotes = down;
                    q.TotalVotes = total;
                    try { await _questionsTable.ExecuteAsync(TableOperation.Replace(q)); }
                    catch { /* nastavi dalje */ }
                }
            });

            // COMMENTS
            await ForEachSegmentAsync(_commentsTable, new TableQuery<Comment>(), async c =>
            {
                var (up, down, total) = await GetVoteStatsAsync(c.RowKey, "COMMENT");
                if (c.Upvotes != up || c.Downvotes != down || c.TotalVotes != total)
                {
                    c.Upvotes = up;
                    c.Downvotes = down;
                    c.TotalVotes = total;
                    try { await _commentsTable.ExecuteAsync(TableOperation.Replace(c)); }
                    catch { /* nastavi dalje */ }
                }
            });
        }

        private static async Task ForEachSegmentAsync<T>(CloudTable table, TableQuery<T> query, Func<T, Task> perItem) where T : TableEntity, new()
        {
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;

                foreach (var item in seg.Results)
                    await perItem(item);
            } while (token != null);
        }
    }
}