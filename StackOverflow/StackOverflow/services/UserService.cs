using Azure.Data.Tables;
using Azure.Storage.Blobs;
using StackOverflow.Models;
using Azure;

namespace StackOverflow.Services
{
    public class UserService
    {
        private readonly TableClient _tableClient;
        private readonly BlobContainerClient _blobContainerClient;

        public UserService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Users");
            _blobContainerClient = new BlobContainerClient(connectionString, "profile-pictures");
            _blobContainerClient.CreateIfNotExists();
        }

        public async Task<User?> GetUserAsync(string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<User>("USER", rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine($"Error getting user {rowKey}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            try
            {
                await foreach (var user in _tableClient.QueryAsync<User>())
                {
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine($"Error getting all users: {ex.Message}");
            }
            return users;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                await _tableClient.AddEntityAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                // Pokušaj sa Merge mode umesto Replace da sačuvaš postojeće podatke
                await _tableClient.UpdateEntityAsync(user, user.ETag, TableUpdateMode.Merge);
            }
            catch (RequestFailedException ex) when (ex.Status == 412) // Precondition Failed - ETag mismatch
            {
                // ETag je zastario, pokušaj ponovo sa najnovijim podacima
                var latestUser = await GetUserAsync(user.RowKey);
                if (latestUser != null)
                {
                    // Kopirati podatke u najnoviju verziju
                    latestUser.FirstName = user.FirstName;
                    latestUser.LastName = user.LastName;
                    latestUser.Username = user.Username;
                    latestUser.Email = user.Email;
                    latestUser.Country = user.Country;
                    latestUser.City = user.City;
                    latestUser.StreetAddress = user.StreetAddress;
                    latestUser.Gender = user.Gender;
                    latestUser.ProfilePictureUrl = user.ProfilePictureUrl;
                    latestUser.IsAdmin = user.IsAdmin;

                    await _tableClient.UpdateEntityAsync(latestUser, latestUser.ETag, TableUpdateMode.Merge);
                }
                else
                {
                    throw new InvalidOperationException("User not found for update");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {user.RowKey}: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            try
            {
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = _blobContainerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile picture for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task IncrementUserQuestionsCountAsync(string userId)
        {
            try
            {
                var user = await GetUserAsync(userId);
                if (user != null)
                {
                    user.QuestionsCount++;
                    await UpdateUserAsync(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error incrementing questions count for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task DecrementUserQuestionsCountAsync(string userId)
        {
            try
            {
                var user = await GetUserAsync(userId);
                if (user != null && user.QuestionsCount > 0)
                {
                    user.QuestionsCount--;
                    await UpdateUserAsync(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrementing questions count for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserQuestionsCountAsync(string userId, int questionsCount)
        {
            try
            {
                var user = await GetUserAsync(userId);
                if (user != null)
                {
                    user.QuestionsCount = questionsCount;
                    await UpdateUserAsync(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating questions count for user {userId}: {ex.Message}");
                throw;
            }
        }
    }
}