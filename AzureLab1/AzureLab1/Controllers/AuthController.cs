using Microsoft.AspNetCore.Mvc;
using MyApi.Models;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly StorageService _storage;

    public AuthController(StorageService storage)
    {
        _storage = storage;
    }

    /// <summary>Logowanie — zwraca token sesji</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Login i hasło są wymagane.");

        var valid = await _storage.ValidateUserAsync(request.Login, request.Password);

        if (!valid)
            return Unauthorized("Nieprawidłowy login lub hasło.");

        var sessionId = await _storage.CreateSessionAsync(request.Login);
        return Ok(new LoginResponse(sessionId));
    }

    /// <summary>Wylogowanie — usuwa sesję użytkownika</summary>
    [HttpDelete("logout/{login}")]
    public async Task<IActionResult> Logout(string login)
    {
        var deleted = await _storage.DeleteSessionAsync(login);

        if (!deleted)
            return NotFound($"Brak aktywnej sesji dla użytkownika '{login}'.");

        return Ok($"Użytkownik '{login}' został wylogowany.");
    }
}
