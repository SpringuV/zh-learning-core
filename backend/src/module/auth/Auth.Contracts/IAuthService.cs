using Auth.Contracts.DTOs;

namespace Auth.Contracts;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<UserRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<UserLoggoutResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> ActivateAccountAsync(ActivateAccountRequest request, CancellationToken cancellationToken);
    Task<bool> ResendActivateAccountAsync(ResendActivationRequest request, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}