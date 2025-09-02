using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using StackOverflow.DTOs;
using StackOverflow.Models;

namespace StackOverflow.Services
{
    public class QuestionService
    {
        private readonly TableClient _tableClient;
        private readonly TableClient _userTableClient;
        private readonly BlobContainerClient _blobContainerClient;

        public QuestionService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Questions");
            _tableClient.CreateIfNotExists();
            _userTableClient = new TableClient(connectionString, "Users");
            _userTableClient.CreateIfNotExists();
            _blobContainerClient = new BlobContainerClient(connectionString, "question-pictures");
            _blobContainerClient.CreateIfNotExists();
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

                questionDetailsList.Add(new QuestionDetails
                {
                    QuestionId = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    TotalVotes = question.TotalVotes,
                    CreatedAt = question.Timestamp?.DateTime ?? question.CreatedDate,
                    User = user != null ? new UserInfo { 
                        Username = user.Username, 
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        QuestionsCount = user.QuestionsCount
                    } : new UserInfo { Username = "Unknown" },
                    AnswersCount = 0 // This needs to be implemented
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

                return new QuestionDetails
                {
                    QuestionId = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    TotalVotes = question.TotalVotes,
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
    }
}
