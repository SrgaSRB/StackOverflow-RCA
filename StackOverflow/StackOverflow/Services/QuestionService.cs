using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using StackOverflow.Models;

namespace StackOverflow.Services
{
    public class QuestionService
    {
        private readonly TableClient _tableClient;
        private readonly BlobContainerClient _blobContainerClient;

        public QuestionService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Questions");
            _tableClient.CreateIfNotExists();
            _blobContainerClient = new BlobContainerClient(connectionString, "question-pictures");
            _blobContainerClient.CreateIfNotExists();
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

        public async Task<List<Question>> GetQuestionsByUserIdAsync(string userId)
        {
            var questions = new List<Question>();
            await foreach (var question in _tableClient.QueryAsync<Question>(q => q.UserId == userId))
            {
                questions.Add(question);
            }
            return questions;
        }
    }
}
