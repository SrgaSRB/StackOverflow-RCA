using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    public class DataService
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly CloudQueueClient _queueClient;
        private readonly CloudTable _questionTable;
        private readonly CloudTable _commentTable;
        private readonly CloudTable _userTable;
        private readonly CloudTable _notificationLogTable;
        private readonly CloudQueue _notificationQueue;

        public DataService()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            Trace.TraceInformation($"Connection string: {connectionString ?? "NULL"}");

            if (string.IsNullOrEmpty(connectionString))
            {
                Trace.TraceError("StorageConnectionString is null or empty!");
                throw new InvalidOperationException("StorageConnectionString configuration is missing");
            }

            _storageAccount = CloudStorageAccount.Parse(connectionString);

            _tableClient = _storageAccount.CreateCloudTableClient();
            _queueClient = _storageAccount.CreateCloudQueueClient();

            // Initialize tables
            _questionTable = _tableClient.GetTableReference("Questions");
            _commentTable = _tableClient.GetTableReference("Comments");
            _userTable = _tableClient.GetTableReference("Users");
            _notificationLogTable = _tableClient.GetTableReference("Notificationlogs");

            // Initialize queue
            _notificationQueue = _queueClient.GetQueueReference("notifications");

            // Create if not exists
            _questionTable.CreateIfNotExists();
            _commentTable.CreateIfNotExists();
            _userTable.CreateIfNotExists();
            _notificationLogTable.CreateIfNotExists();
            _notificationQueue.CreateIfNotExists();
        }

        public async Task<NotificationMessage> ReceiveNotificationAsync()
        {
            try
            {
                var message = await _notificationQueue.GetMessageAsync();
                if (message != null)
                {
                    var notification = JsonConvert.DeserializeObject<NotificationMessage>(message.AsString);
                    await _notificationQueue.DeleteMessageAsync(message);
                    return notification;
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error receiving notification message: {ex.Message}");
                return null;
            }
        }

        public async Task<Question> GetQuestionAsync(string questionId)
        {
            try
            {
                var operation = TableOperation.Retrieve<Question>("QUESTION", questionId);
                var result = await _questionTable.ExecuteAsync(operation);
                return result.Result as Question;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting question {questionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<Comment> GetCommentAsync(string commentId)
        {
            try
            {
                var newQuestion = new Question();
                newQuestion.PartitionKey = "QUESTION";
                newQuestion.RowKey = Guid.NewGuid().ToString();
                newQuestion.Title = "Sample Question Title";
                newQuestion.Description = "Sample question text.";
                newQuestion.UserId = "user123";
                newQuestion.Upvotes = 0;
                newQuestion.Downvotes = 0;
                newQuestion.TotalVotes = 0;
                newQuestion.BestCommentId = "";
                newQuestion.Timestamp = DateTimeOffset.UtcNow;
                newQuestion.PictureUrl = "";

                var addOperation = TableOperation.Insert(newQuestion);
                await _questionTable.ExecuteAsync(addOperation);

                var operation = TableOperation.Retrieve<Comment>("COMMENT", commentId);
                var result = await _commentTable.ExecuteAsync(operation);
                return result.Result as Comment;
            }
            catch (StorageException ex)
            {
                Trace.TraceError($"Storage Exception: {ex.Message}");
                Trace.TraceError($"Request Information: {ex.RequestInformation?.HttpStatusMessage}");
                Trace.TraceError($"Extended Error: {ex.RequestInformation?.ExtendedErrorInformation?.ErrorMessage}");
                return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting comment {commentId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Comment>> GetCommentsForQuestionAsync(string questionId)
        {
            try
            {
                var query = new TableQuery<Comment>().Where(
                    TableQuery.GenerateFilterCondition("QuestionId", QueryComparisons.Equal, questionId)
                );

                var comments = new List<Comment>();
                TableContinuationToken token = null;

                do
                {
                    var segment = await _commentTable.ExecuteQuerySegmentedAsync(query, token);
                    comments.AddRange(segment.Results);
                    token = segment.ContinuationToken;
                } while (token != null);

                return comments;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting comments for question {questionId}: {ex.Message}");
                return new List<Comment>();
            }
        }

        public async Task<User> GetUserAsync(string userId)
        {
            try
            {
                var operation = TableOperation.Retrieve<User>("USER", userId);
                var result = await _userTable.ExecuteAsync(operation);
                return result.Result as User;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetUsersAsync(List<string> userIds)
        {
            var users = new List<User>();
            foreach (var userId in userIds)
            {
                var user = await GetUserAsync(userId);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            return users;
        }

        public async Task SaveNotificationLogAsync(NotificationLog log)
        {
            try
            {
                var operation = TableOperation.Insert(log);
                await _notificationLogTable.ExecuteAsync(operation);
                Trace.TraceInformation($"Notification log saved for answer {log.AnswerId}");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error saving notification log for answer {log.AnswerId}: {ex.Message}");
            }
        }
    }
}