using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class NotificationQueueService
    {

        private readonly QueueClient _queueClient;

        public NotificationQueueService(string connectionString)
        {
            _queueClient = new QueueClient(connectionString, "notifications");
            _queueClient.CreateIfNotExists();
        }

    }
}