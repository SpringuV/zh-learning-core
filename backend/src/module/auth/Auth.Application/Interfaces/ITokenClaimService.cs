namespace Auth.Application.Interfaces;

public record TokenResult(ValidateUser Users, string AccessToken, string RefreshToken);
public interface ITokenClaimService
{
    Task<TokenResult> GetTokenAsync(ValidateUser user, CancellationToken cancellationToken);
    Task<TokenResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task<bool> RevokeAsync(string refreshToken, CancellationToken cancellationToken);
}
