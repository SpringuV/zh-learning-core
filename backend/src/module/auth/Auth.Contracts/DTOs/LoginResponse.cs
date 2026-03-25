namespace Auth.Contracts.DTOs;

public record LoginResponse(
    string Message,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
