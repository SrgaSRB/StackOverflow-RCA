using Azure.Storage.Queues;
using StackOverflow.Models;
using System.Text.Json;

namespace StackOverflow.Services
{
    public class NotificationQueueService
    {
        private readonly QueueClient _queueClient;

        public NotificationQueueService(string connectionString)
        {
            _queueClient = new QueueClient(connectionString, "notifications");
            _queueClient.CreateIfNotExists();
        }

        public async Task SendNotificationAsync(NotificationMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var encodedMessage = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            await _queueClient.SendMessageAsync(encodedMessage);
        }
    }
}
