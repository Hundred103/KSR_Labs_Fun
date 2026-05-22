using Microsoft.AspNetCore.Mvc;
using MyApi.Models;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly StorageService _storage;

    public UsersController(StorageService storage)
    {
        _storage = storage;
    }

    /// <summary>Rejestracja nowego użytkownika</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Login i hasło są wymagane.");

        var success = await _storage.RegisterUserAsync(request.Login, request.Password);

        if (!success)
            return Conflict($"Użytkownik '{request.Login}' już istnieje.");

        return Ok($"Użytkownik '{request.Login}' został zarejestrowany.");
    }
}
