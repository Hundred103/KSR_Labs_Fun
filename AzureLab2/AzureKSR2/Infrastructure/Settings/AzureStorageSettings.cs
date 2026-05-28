namespace Infrastructure.Settings;

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InputContainer { get; set; } = "original-files";
    public string OutputContainer { get; set; } = "encoded-files";
    public string QueueName { get; set; } = "processing-queue";
}