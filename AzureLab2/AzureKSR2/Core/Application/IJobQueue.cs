namespace Core.Application;

public interface IJobQueue
{
    Task SendAsync(string blobName, CancellationToken ct = default);
    Task<(string MessageId, string MessageText, string PopReceipt)?> ReceiveAsync(CancellationToken ct = default);
    Task DeleteAsync(string messageId, string popReceipt, CancellationToken ct = default);
}