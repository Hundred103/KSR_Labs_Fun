using Azure.Storage.Queues;
using Core.Application;
using Core.Domain;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services;

public class AzureJobQueue : IJobQueue
{
    private readonly QueueClient _queueClient;

    public AzureJobQueue(AzureStorageSettings settings)
    {
        _queueClient = new QueueClient(
            settings.ConnectionString,
            settings.QueueName,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    }
    public async Task SendAsync(string blobName, CancellationToken ct = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: ct);
        var job = new FileJob { BlobName = blobName };
        var json = JsonSerializer.Serialize(job);
        await _queueClient.SendMessageAsync(json, cancellationToken: ct);
    }

    public async Task<(string MessageId, string MessageText, string PopReceipt)?> ReceiveAsync(CancellationToken ct = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var response = await _queueClient.ReceiveMessageAsync(
            visibilityTimeout: TimeSpan.FromSeconds(3),
            cancellationToken: ct);

        if (response.Value is null) return null;

        return (response.Value.MessageId, response.Value.MessageText, response.Value.PopReceipt);
    }

    public async Task DeleteAsync(string messageId, string popReceipt, CancellationToken ct = default)
    {
        await _queueClient.DeleteMessageAsync(messageId, popReceipt, ct);
    }
}