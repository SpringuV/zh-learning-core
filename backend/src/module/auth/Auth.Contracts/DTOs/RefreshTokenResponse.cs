
namespace Auth.Contracts.DTOs;

public record RefreshTokenResponse(
    string Message,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
