using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using MyApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace MyApi.Services;

public class StorageService
{
    private readonly TableServiceClient _tableService;
    private readonly BlobServiceClient _blobService;
    private const string UsersTable = "users";
    private const string SessionsTable = "sessions";
    private const string FilesContainer = "userfiles";

    public StorageService(TableServiceClient tableService, BlobServiceClient blobService)
    {
        _tableService = tableService;
        _blobService = blobService;
    }

    // --------------- USERS ---------------

    public async Task<bool> RegisterUserAsync(string login, string password)
    {
        var table = _tableService.GetTableClient(UsersTable);
        await table.CreateIfNotExistsAsync();

        // sprawdź czy login już istnieje
        try
        {
            await table.GetEntityAsync<UserEntity>("users", login);
            return false; // już istnieje
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // nie istnieje - możemy dodać
        }

        var entity = new UserEntity
        {
            PartitionKey = "users",
            RowKey = login,
            PasswordHash = HashPassword(password)
        };

        await table.AddEntityAsync(entity);
        return true;
    }

    public async Task<bool> ValidateUserAsync(string login, string password)
    {
        var table = _tableService.GetTableClient(UsersTable);

        try
        {
            var response = await table.GetEntityAsync<UserEntity>("users", login);
            return response.Value.PasswordHash == HashPassword(password);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    // --------------- SESSIONS ---------------

    public async Task<string> CreateSessionAsync(string login)
    {
        var table = _tableService.GetTableClient(SessionsTable);
        await table.CreateIfNotExistsAsync();

        var sessionId = Guid.NewGuid().ToString();

        var entity = new SessionEntity
        {
            PartitionKey = "sessions",
            RowKey = login,
            SessionId = sessionId
        };

        // upsert - nadpisuje poprzednią sesję jeśli istnieje
        await table.UpsertEntityAsync(entity);
        return sessionId;
    }

    public async Task<bool> DeleteSessionAsync(string login)
    {
        var table = _tableService.GetTableClient(SessionsTable);

        try
        {
            await table.DeleteEntityAsync("sessions", login);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<bool> ValidateSessionAsync(string login, string sessionId)
    {
        var table = _tableService.GetTableClient(SessionsTable);

        try
        {
            var response = await table.GetEntityAsync<SessionEntity>("sessions", login);
            return response.Value.SessionId == sessionId;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    // --------------- BLOBS ---------------

    public async Task SaveFileAsync(string fileName, string content)
    {
        var container = _blobService.GetBlobContainerClient(FilesContainer);
        await container.CreateIfNotExistsAsync();

        var blob = container.GetBlobClient(fileName);
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);
        await blob.UploadAsync(stream, overwrite: true);
    }

    public async Task<string?> GetFileAsync(string fileName)
    {
        var container = _blobService.GetBlobContainerClient(FilesContainer);
        var blob = container.GetBlobClient(fileName);

        try
        {
            var response = await blob.DownloadContentAsync();
            return response.Value.Content.ToString();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    // --------------- HELPERS ---------------

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
