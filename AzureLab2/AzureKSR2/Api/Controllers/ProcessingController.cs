using Core.Application;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Controllers;

[ApiController]
[Route("api/processing")]
public class ProcessingController : ControllerBase
{
    private readonly IFileStorage _storage;
    private readonly IJobQueue _queue;
    private readonly AzureStorageSettings _settings;

    public ProcessingController(
        IFileStorage storage,
        IJobQueue queue,
        IOptions<AzureStorageSettings> options)
    {
        _storage = storage;
        _queue = queue;
        _settings = options.Value;
    }

    [HttpPost("encode")]
    public async Task<IActionResult> Encode([FromBody] EncodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.Content is null)
            return BadRequest("Brak nazwy pliku lub treści.");

        await _storage.UploadAsync(_settings.InputContainer, request.FileName, request.Content, ct);
        await _queue.SendAsync(request.FileName, ct);

        return Ok(new { message = "Przyjęto do przetworzenia.", file = request.FileName });
    }

    [HttpGet("download/{name}")]
    public async Task<IActionResult> Download(string name, CancellationToken ct)
    {
        try
        {
            var content = await _storage.DownloadAsync(_settings.OutputContainer, name, ct);
            return Ok(content);
        }
        catch
        {
            return NotFound($"Plik '{name}' nie jest jeszcze gotowy.");
        }
    }
}

public record EncodeRequest(string FileName, string Content);