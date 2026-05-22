using Microsoft.AspNetCore.Mvc;
using MyApi.Models;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly StorageService _storage;

    public FilesController(StorageService storage)
    {
        _storage = storage;
    }

    /// <summary>Zapisuje plik w Blob Storage (wymaga aktywnej sesji)</summary>
    [HttpPost]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.SessionId) ||
            string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest("Login, SessionId i FileName są wymagane.");

        var sessionValid = await _storage.ValidateSessionAsync(request.Login, request.SessionId);
        if (!sessionValid)
            return Unauthorized("Nieprawidłowy lub wygasły identyfikator sesji.");

        await _storage.SaveFileAsync(request.FileName, request.Content ?? string.Empty);
        return Ok($"Plik '{request.FileName}' został zapisany.");
    }

    /// <summary>Odczytuje plik z Blob Storage (wymaga aktywnej sesji)</summary>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetFile(
        string name,
        [FromQuery] string login,
        [FromQuery] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(sessionId))
            return BadRequest("Parametry login i sessionId są wymagane.");

        var sessionValid = await _storage.ValidateSessionAsync(login, sessionId);
        if (!sessionValid)
            return Unauthorized("Nieprawidłowy lub wygasły identyfikator sesji.");

        var content = await _storage.GetFileAsync(name);
        if (content is null)
            return NotFound($"Plik '{name}' nie istnieje.");

        return Ok(content);
    }
}
