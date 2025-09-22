using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class UserService
    {
        private readonly CloudTable _usersTable;
        private readonly CloudBlobContainer _blobContainer;

        public UserService(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Table
            var tableClient = storageAccount.CreateCloudTableClient();
            _usersTable = tableClient.GetTableReference("Users");
            _usersTable.CreateIfNotExists();

            // Blob
            var blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("profile-pictures");
            _blobContainer.CreateIfNotExists();
        }

        public async Task<User> GetUserAsync(string rowKey)
        {
            try
            {
                var retrieve = TableOperation.Retrieve<User>("USER", rowKey);
                var result = await _usersTable.ExecuteAsync(retrieve);
                return result.Result as User;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting user {rowKey}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            try
            {
                TableContinuationToken token = null;
                do
                {
                    var query = new TableQuery<User>();
                    var segment = await _usersTable.ExecuteQuerySegmentedAsync(query, token);
                    users.AddRange(segment.Results);
                    token = segment.ContinuationToken;
                } while (token != null);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting all users: {ex.Message}");
            }
            return users;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                var insert = TableOperation.Insert(user);
                await _usersTable.ExecuteAsync(insert);
                return user;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                var merge = TableOperation.InsertOrMerge(user);
                await _usersTable.ExecuteAsync(merge);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error updating user {user.RowKey}: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadProfilePictureAsync(string userId, Stream fileStream, string originalFileName)
        {
            try
            {
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
                var blob = _blobContainer.GetBlockBlobReference(fileName);

                await blob.UploadFromStreamAsync(fileStream);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error uploading profile picture for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task IncrementUserQuestionsCountAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            if (user != null)
            {
                user.QuestionsCount++;
                await UpdateUserAsync(user);
            }
        }

        public async Task DecrementUserQuestionsCountAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            if (user != null && user.QuestionsCount > 0)
            {
                user.QuestionsCount--;
                await UpdateUserAsync(user);
            }
        }

        public async Task UpdateUserQuestionsCountAsync(string userId, int questionsCount)
        {
            var user = await GetUserAsync(userId);
            if (user != null)
            {
                user.QuestionsCount = questionsCount;
                await UpdateUserAsync(user);
            }
        }
    }
}