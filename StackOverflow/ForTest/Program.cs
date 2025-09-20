using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = "UseDevelopmentStorage=true";
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("notifications");

            await queue.CreateIfNotExistsAsync();

            var testMessage = new
            {
                AnswerId = "test-answer-id",
                QuestionId = "test-question-id",
                Timestamp = DateTime.UtcNow
            };

            var messageJson = JsonConvert.SerializeObject(testMessage);
            var cloudMessage = new CloudQueueMessage(messageJson);
            await queue.AddMessageAsync(cloudMessage);

            Console.WriteLine("Test message sent to notifications queue!");
        }
    }
}
