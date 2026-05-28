using Azure.Storage.Blobs;
using Core.Application;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text;

namespace Infrastructure.Services;

public class BlobFileStorage : IFileStorage
{
    private readonly AzureStorageSettings _settings;

    public BlobFileStorage(AzureStorageSettings settings)
    {
        _settings = settings;
    }

    public async Task UploadAsync(string containerName, string blobName, string content, CancellationToken ct = default)
    {
        var client = new BlobContainerClient(_settings.ConnectionString, containerName);
        await client.CreateIfNotExistsAsync(cancellationToken: ct);

        var blob = client.GetBlobClient(blobName);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await blob.UploadAsync(stream, overwrite: true, cancellationToken: ct);
    }

    public async Task<string> DownloadAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        var client = new BlobContainerClient(_settings.ConnectionString, containerName);
        var blob = client.GetBlobClient(blobName);

        var response = await blob.DownloadContentAsync(cancellationToken: ct);
        return response.Value.Content.ToString();
    }
}