using Azure;
using Azure.Data.Tables;

namespace MyApi.Models;

public class SessionEntity : ITableEntity
{
    // PartitionKey = "sessions"
    // RowKey = login użytkownika
    public string PartitionKey { get; set; } = "sessions";
    public string RowKey { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
