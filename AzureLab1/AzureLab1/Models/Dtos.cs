namespace MyApi.Models;

public record RegisterRequest(string Login, string Password);

public record LoginRequest(string Login, string Password);

public record LoginResponse(string SessionId);

public record SaveFileRequest(string Login, string SessionId, string FileName, string Content);

public record GetFileRequest(string Login, string SessionId);
