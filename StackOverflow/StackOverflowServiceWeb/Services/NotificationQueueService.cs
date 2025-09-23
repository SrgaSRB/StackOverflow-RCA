using Azure.Storage.Queues;
using Common.Models;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

public sealed class NotificationQueueService
{
    private readonly QueueClient _queue;

    public NotificationQueueService(string connectionString)
    {
        var opts = new QueueClientOptions(QueueClientOptions.ServiceVersion.V2021_02_12)
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queue = new QueueClient(connectionString, "notifications", opts);
        _queue.CreateIfNotExists(); 
    }

    public async Task SendNotificationAsync(NotificationMessage notification, CancellationToken ct = default)
    {
        var msg = JsonConvert.SerializeObject(notification);
        await _queue.SendMessageAsync(msg);

        await _queue.SendMessageAsync(msg, ct);
    }
}
