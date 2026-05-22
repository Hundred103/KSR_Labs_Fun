using Azure;
using Azure.Data.Tables;

namespace MyApi.Models;

public class UserEntity : ITableEntity
{
    // PartitionKey = "users" (stała grupa)
    // RowKey = login (unikalny identyfikator użytkownika)
    public string PartitionKey { get; set; } = "users";
    public string RowKey { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
