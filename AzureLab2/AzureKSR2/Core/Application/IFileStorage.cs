namespace Core.Application;

public interface IFileStorage
{
    Task UploadAsync(string containerName, string blobName, string content, CancellationToken ct = default);
    Task<string> DownloadAsync(string containerName, string blobName, CancellationToken ct = default);
}