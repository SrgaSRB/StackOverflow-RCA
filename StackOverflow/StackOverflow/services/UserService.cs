using Azure.Data.Tables;
using StackOverflow.Models;

namespace StackOverflow.Services
{
    public class UserService
    {
        private readonly TableClient _tableClient;

        public UserService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Users");
        }

        public async Task<User?> GetUserAsync(string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<User>("USER", rowKey);
                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            await foreach (var user in _tableClient.QueryAsync<User>())
            {
                users.Add(user);
            }
            return users;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _tableClient.AddEntityAsync(user);
            return user;
        }
    }
}