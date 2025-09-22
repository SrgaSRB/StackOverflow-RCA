using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;

namespace StackOverflowServiceWeb.Services
{
    public class NotificationQueueService
    {

        private readonly CloudQueue _queue;

        public NotificationQueueService(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudQueueClient();
            _queue = client.GetQueueReference("notifications");
            _queue.CreateIfNotExists();
        }

        public async Task SendNotificationAsync(string payload)
        {
            var msg = new CloudQueueMessage(payload);
            await _queue.AddMessageAsync(msg);
        }

    }
}