using Core.Application;
using Core.Domain;
using Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Api.Workers;

public class QueueProcessorWorker : BackgroundService
{
    private readonly IJobQueue _queue;
    private readonly IFileStorage _storage;
    private readonly AzureStorageSettings _settings;
    private readonly ILogger<QueueProcessorWorker> _logger;
    private static readonly Random _rng = new();

    public QueueProcessorWorker(
        IJobQueue queue,
        IFileStorage storage,
        AzureStorageSettings settings,
        ILogger<QueueProcessorWorker> logger)
    {
        _queue = queue;
        _storage = storage;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker uruchomiony.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var received = await _queue.ReceiveAsync(stoppingToken);

                if (received is null)
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                var (messageId, messageText, popReceipt) = received.Value;
                var job = JsonSerializer.Deserialize<FileJob>(messageText)!;

                _logger.LogInformation("Odebrano zadanie: {Blob}", job.BlobName);

                // Symulacja awarii: 1/3 szans na wyjątek
                if (_rng.Next(3) == 0)
                {
                    _logger.LogWarning("AWARIA węzła! Wiadomość wróci do kolejki za 3s. Plik: {Blob}", job.BlobName);
                    throw new InvalidOperationException(
                        $"Symulacja awarii węzła dla: {job.BlobName} @ {DateTime.UtcNow:O}");
                }

                var original = await _storage.DownloadAsync(_settings.InputContainer, job.BlobName, stoppingToken);
                var encoded = Rot13Cipher.Encode(original);
                await _storage.UploadAsync(_settings.OutputContainer, job.BlobName, encoded, stoppingToken);

                await _queue.DeleteAsync(messageId, popReceipt, stoppingToken);
                _logger.LogInformation("Sukces: {Blob} zakodowany i usunięty z kolejki.", job.BlobName);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Wyjątek logujemy ale NIE łapiemy cicho — Worker kontynuuje pętlę
                // Wiadomość automatycznie wróci do kolejki po upływie visibilityTimeout (3s)
                _logger.LogError(ex, "Błąd przetwarzania. Wiadomość wróci do kolejki za 3 sekundy.");
            }
        }
    }
}