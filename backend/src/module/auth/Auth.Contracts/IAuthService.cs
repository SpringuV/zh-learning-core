using Auth.Contracts.DTOs;

namespace Auth.Contracts;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<UserRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}