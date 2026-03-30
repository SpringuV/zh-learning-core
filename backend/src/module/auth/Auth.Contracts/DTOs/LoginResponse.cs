namespace Auth.Contracts.DTOs;

public record LoginResponse(
    
    string UserId,
    string UserName,
    IReadOnlyList<string> Roles,
    string Message,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
