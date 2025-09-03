using Azure;
using Azure.Data.Tables;
using StackOverflow.Models;

namespace StackOverflow.Services
{
    public class VoteService
    {
        private readonly TableClient _tableClient;
        private readonly TableClient _questionTableClient;
        private readonly TableClient _commentTableClient;

        public VoteService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Votes");
            _tableClient.CreateIfNotExists();
            _questionTableClient = new TableClient(connectionString, "Questions");
            _questionTableClient.CreateIfNotExists();
            _commentTableClient = new TableClient(connectionString, "Comments");
            _commentTableClient.CreateIfNotExists();
        }

        public async Task<Vote?> GetUserVoteAsync(string userId, string targetId, string targetType)
        {
            try
            {
                await foreach (var vote in _tableClient.QueryAsync<Vote>(v => 
                    v.UserId == userId && v.TargetId == targetId && v.TargetType == targetType))
                {
                    return vote;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> GetVoteStatsAsync(string targetId, string targetType)
        {
            int upvotes = 0;
            int downvotes = 0;

            await foreach (var vote in _tableClient.QueryAsync<Vote>(v => 
                v.TargetId == targetId && v.TargetType == targetType))
            {
                if (vote.IsUpvote)
                    upvotes++;
                else
                    downvotes++;
            }

            return (upvotes, downvotes, upvotes + downvotes);
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> VoteAsync(string userId, string targetId, string targetType, bool isUpvote)
        {
            // Check if user already voted
            var existingVote = await GetUserVoteAsync(userId, targetId, targetType);

            if (existingVote != null)
            {
                // If same vote type, remove the vote (toggle off)
                if (existingVote.IsUpvote == isUpvote)
                {
                    await _tableClient.DeleteEntityAsync(existingVote.PartitionKey, existingVote.RowKey);
                }
                else
                {
                    // If different vote type, update the vote
                    existingVote.IsUpvote = isUpvote;
                    await _tableClient.UpdateEntityAsync(existingVote, existingVote.ETag, TableUpdateMode.Replace);
                }
            }
            else
            {
                // Create new vote
                var newVote = new Vote
                {
                    RowKey = Guid.NewGuid().ToString(),
                    UserId = userId,
                    TargetId = targetId,
                    TargetType = targetType,
                    IsUpvote = isUpvote,
                    Timestamp = DateTimeOffset.UtcNow
                };
                await _tableClient.AddEntityAsync(newVote);
            }

            // Get updated vote stats
            var (upvotes, downvotes, totalVotes) = await GetVoteStatsAsync(targetId, targetType);

            // Update the corresponding Question or Comment entity
            await UpdateTargetVoteCountsAsync(targetId, targetType, upvotes, downvotes, totalVotes);

            return (upvotes, downvotes, totalVotes);
        }

        private async Task UpdateTargetVoteCountsAsync(string targetId, string targetType, int upvotes, int downvotes, int totalVotes)
        {
            try
            {
                if (targetType == "QUESTION")
                {
                    var response = await _questionTableClient.GetEntityAsync<Question>("QUESTION", targetId);
                    var question = response.Value;
                    question.Upvotes = upvotes;
                    question.Downvotes = downvotes;
                    question.TotalVotes = totalVotes;
                    await _questionTableClient.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);
                }
                else if (targetType == "COMMENT")
                {
                    var response = await _commentTableClient.GetEntityAsync<Comment>("COMMENT", targetId);
                    var comment = response.Value;
                    comment.Upvotes = upvotes;
                    comment.Downvotes = downvotes;
                    comment.TotalVotes = totalVotes;
                    await _commentTableClient.UpdateEntityAsync(comment, comment.ETag, TableUpdateMode.Replace);
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Target entity not found, skip update
            }
            catch (Exception)
            {
                // Log error but don't fail the vote operation
            }
        }

        public async Task<string?> GetUserVoteTypeAsync(string userId, string targetId, string targetType)
        {
            var vote = await GetUserVoteAsync(userId, targetId, targetType);
            if (vote == null)
                return null;
            return vote.IsUpvote ? "upvote" : "downvote";
        }

        // Method to migrate existing data and update vote counts in Question/Comment entities
        public async Task MigrateVoteCountsAsync()
        {
            // Update all questions
            await foreach (var question in _questionTableClient.QueryAsync<Question>())
            {
                var (upvotes, downvotes, totalVotes) = await GetVoteStatsAsync(question.RowKey, "QUESTION");
                if (question.Upvotes != upvotes || question.Downvotes != downvotes || question.TotalVotes != totalVotes)
                {
                    question.Upvotes = upvotes;
                    question.Downvotes = downvotes;
                    question.TotalVotes = totalVotes;
                    try
                    {
                        await _questionTableClient.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);
                    }
                    catch (Exception)
                    {
                        // Continue with next entity if update fails
                    }
                }
            }

            // Update all comments
            await foreach (var comment in _commentTableClient.QueryAsync<Comment>())
            {
                var (upvotes, downvotes, totalVotes) = await GetVoteStatsAsync(comment.RowKey, "COMMENT");
                if (comment.Upvotes != upvotes || comment.Downvotes != downvotes || comment.TotalVotes != totalVotes)
                {
                    comment.Upvotes = upvotes;
                    comment.Downvotes = downvotes;
                    comment.TotalVotes = totalVotes;
                    try
                    {
                        await _commentTableClient.UpdateEntityAsync(comment, comment.ETag, TableUpdateMode.Replace);
                    }
                    catch (Exception)
                    {
                        // Continue with next entity if update fails
                    }
                }
            }
        }
    }
}
