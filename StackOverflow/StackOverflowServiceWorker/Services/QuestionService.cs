using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using StackOverflow.DTOs;
using StackOverflow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackOverflow.Services
{
    public class QuestionService
    {
        private readonly TableClient _tableClient;
        private readonly TableClient _userTableClient;
        private readonly TableClient _commentsTableClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly VoteService _voteService;

        public QuestionService(string connectionString, VoteService voteService)
        {
            _tableClient = new TableClient(connectionString, "Questions");
            _tableClient.CreateIfNotExists();
            _userTableClient = new TableClient(connectionString, "Users");
            _userTableClient.CreateIfNotExists();
            _commentsTableClient = new TableClient(connectionString, "Comments");
            _commentsTableClient.CreateIfNotExists();
            _blobContainerClient = new BlobContainerClient(connectionString, "question-pictures");
            _blobContainerClient.CreateIfNotExists();
            _voteService = voteService;
        }

        public async Task<List<QuestionDetails>> GetAllQuestionsWithUserDetailsAsync()
        {
            var questions = new List<Question>();
            
            // Query all questions
            await foreach (var question in _tableClient.QueryAsync<Question>())
            {
                questions.Add(question);
            }

            // Sort questions by CreatedDate in descending order (newest first)
            questions = questions.OrderByDescending(q => q.CreatedDate).ToList();

            var questionDetailsList = new List<QuestionDetails>();
            foreach (var question in questions)
            {
                User? user = null;
                try
                {
                    var response = await _userTableClient.GetEntityAsync<User>("USER", question.UserId);
                    user = response.Value;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // User not found, handle as needed
                }

                // Get vote stats for this question (use cached values from entity if available)
                var upvotes = question.Upvotes;
                var downvotes = question.Downvotes;
                var totalVotes = question.TotalVotes;
                
                // If vote counts are all zero, fall back to calculating from Vote table (for existing data)
                if (upvotes == 0 && downvotes == 0 && totalVotes == 0)
                {
                    (upvotes, downvotes, totalVotes) = await _voteService.GetVoteStatsAsync(question.RowKey, "QUESTION");
                }

                // Count answers (comments) for this question
                int answersCount = 0;
                await foreach (var comment in _commentsTableClient.QueryAsync<Comment>(c => c.QuestionId == question.RowKey))
                {
                    answersCount++;
                }

                questionDetailsList.Add(new QuestionDetails
                {
                    QuestionId = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    Upvotes = upvotes,
                    Downvotes = downvotes,
                    TotalVotes = totalVotes,
                    CreatedAt = question.Timestamp?.DateTime ?? question.CreatedDate,
                    User = user != null ? new UserInfo { 
                        Username = user.Username, 
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        QuestionsCount = user.QuestionsCount
                    } : new UserInfo { Username = "Unknown" },
                    AnswersCount = answersCount
                });
            }

            return questionDetailsList;
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            await _tableClient.AddEntityAsync(question);
            return question;
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            await _tableClient.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);
        }

        public async Task<string> UploadQuestionPictureAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task DeletePictureAsync(string pictureUrl)
        {
            if (!string.IsNullOrEmpty(pictureUrl))
            {
                try
                {
                    var uri = new Uri(pictureUrl);
                    var fileName = Path.GetFileName(uri.LocalPath);
                    var blobClient = _blobContainerClient.GetBlobClient(fileName);
                    await blobClient.DeleteIfExistsAsync();
                }
                catch (Exception)
                {
                    // Log the exception, but don't fail the operation
                }
            }
        }

        public async Task<List<Question>> GetQuestionsByUserIdAsync(string userId)
        {
            var questions = new List<Question>();
            await foreach (var question in _tableClient.QueryAsync<Question>(q => q.UserId == userId))
            {
                questions.Add(question);
            }
            return questions;
        }

        public async Task<List<QuestionDetails>> GetQuestionsByUserIdWithDetailsAsync(string userId)
        {
            var questions = new List<Question>();
            await foreach (var question in _tableClient.QueryAsync<Question>(q => q.UserId == userId))
            {
                questions.Add(question);
            }

            // Sort questions by CreatedDate in descending order (newest first)
            questions = questions.OrderByDescending(q => q.CreatedDate).ToList();

            var questionDetailsList = new List<QuestionDetails>();
            foreach (var question in questions)
            {
                User? user = null;
                try
                {
                    var response = await _userTableClient.GetEntityAsync<User>("USER", question.UserId);
                    user = response.Value;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // User not found, handle as needed
                }

                // Get vote stats for this question (use cached values from entity if available)
                var upvotes = question.Upvotes;
                var downvotes = question.Downvotes;
                var totalVotes = question.TotalVotes;
                
                // If vote counts are all zero, fall back to calculating from Vote table (for existing data)
                if (upvotes == 0 && downvotes == 0 && totalVotes == 0)
                {
                    (upvotes, downvotes, totalVotes) = await _voteService.GetVoteStatsAsync(question.RowKey, "QUESTION");
                }

                // Count answers (comments) for this question
                int answersCount = 0;
                await foreach (var comment in _commentsTableClient.QueryAsync<Comment>(c => c.QuestionId == question.RowKey))
                {
                    answersCount++;
                }

                questionDetailsList.Add(new QuestionDetails
                {
                    QuestionId = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    Upvotes = upvotes,
                    Downvotes = downvotes,
                    TotalVotes = totalVotes,
                    CreatedAt = question.Timestamp?.DateTime ?? question.CreatedDate,
                    User = user != null ? new UserInfo { 
                        Username = user.Username, 
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        QuestionsCount = user.QuestionsCount
                    } : new UserInfo { Username = "Unknown" },
                    AnswersCount = answersCount
                });
            }

            return questionDetailsList;
        }

        public async Task<Question?> GetQuestionByIdAsync(string questionId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Question>("QUESTION", questionId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<QuestionDetails?> GetQuestionWithDetailsAsync(string questionId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Question>("QUESTION", questionId);
                var question = response.Value;

                // Get user details
                User? user = null;
                int actualQuestionsCount = 0;
                try
                {
                    var userResponse = await _userTableClient.GetEntityAsync<User>("USER", question.UserId);
                    user = userResponse.Value;
                    
                    // Count actual questions for this user
                    await foreach (var userQuestion in _tableClient.QueryAsync<Question>(q => q.UserId == question.UserId))
                    {
                        actualQuestionsCount++;
                    }
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // User not found, handle as needed
                }

                // Get vote stats for this question (use cached values from entity if available)
                var upvotes = question.Upvotes;
                var downvotes = question.Downvotes;
                var totalVotes = question.TotalVotes;
                
                // If vote counts are all zero, fall back to calculating from Vote table (for existing data)
                if (upvotes == 0 && downvotes == 0 && totalVotes == 0)
                {
                    (upvotes, downvotes, totalVotes) = await _voteService.GetVoteStatsAsync(question.RowKey, "QUESTION");
                }

                return new QuestionDetails
                {
                    QuestionId = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    Upvotes = upvotes,
                    Downvotes = downvotes,
                    TotalVotes = totalVotes,
                    CreatedAt = question.Timestamp?.DateTime ?? question.CreatedDate,
                    User = user != null ? new UserInfo { 
                        Username = user.Username, 
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        QuestionsCount = actualQuestionsCount
                    } : new UserInfo { Username = "Unknown" },
                    Answers = new List<DTOs.Answer>() // Will be populated by controller
                };
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task DeleteQuestionAsync(string questionId)
        {
            var question = await GetQuestionByIdAsync(questionId);
            if (question != null)
            {
                // Delete the picture from blob storage if it exists
                if (!string.IsNullOrEmpty(question.PictureUrl))
                {
                    try
                    {
                        var uri = new Uri(question.PictureUrl);
                        var fileName = Path.GetFileName(uri.LocalPath);
                        var blobClient = _blobContainerClient.GetBlobClient(fileName);
                        await blobClient.DeleteIfExistsAsync();
                    }
                    catch (Exception)
                    {
                        // Log the exception, but don't fail the deletion
                    }
                }

                // Delete the question from table storage
                await _tableClient.DeleteEntityAsync("QUESTION", questionId);
            }
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> UpvoteQuestionAsync(string questionId, string userId)
        {
            return await _voteService.VoteAsync(userId, questionId, "QUESTION", true);
        }

        public async Task<(int upvotes, int downvotes, int totalVotes)> DownvoteQuestionAsync(string questionId, string userId)
        {
            return await _voteService.VoteAsync(userId, questionId, "QUESTION", false);
        }

        public async Task<string?> GetUserVoteAsync(string userId, string questionId)
        {
            return await _voteService.GetUserVoteTypeAsync(userId, questionId, "QUESTION");
        }

        public async Task<List<QuestionDetails>> GetPopularQuestionsAsync(int limit = 5)
        {
            var allQuestions = await GetAllQuestionsWithUserDetailsAsync();
            
            // Sort by total votes descending and then by creation date for ties
            var sortedQuestions = allQuestions
                .OrderByDescending(q => q.TotalVotes)
                .ThenByDescending(q => q.CreatedAt)
                .ToList();
            
            // If limit is high (like 100), return all questions, otherwise apply limit
            if (limit >= 100) 
            {
                return sortedQuestions;
            }
            
            return sortedQuestions.Take(limit).ToList();
        }

        public async Task MarkBestAnswerAsync(string questionId, string answerId)
        {
            var question = await GetQuestionByIdAsync(questionId);
            if (question != null)
            {
                question.BestCommentId = answerId;
                await _tableClient.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);
            }
        }

        public async Task UnmarkBestAnswerAsync(string questionId)
        {
            var question = await GetQuestionByIdAsync(questionId);
            if (question != null)
            {
                question.BestCommentId = null;
                await _tableClient.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);
            }
        }
    }
}
