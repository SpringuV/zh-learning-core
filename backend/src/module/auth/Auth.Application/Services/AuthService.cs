using Auth.Contracts;
using Auth.Contracts.DTOs;
using Auth.Application.Command.Login;
using System.IdentityModel.Tokens.Jwt;
using Auth.Application.Command.Refresh;
using Auth.Application.Command.Register;
using Auth.Application.Command.Logout;
using Auth.Application.Command.ActivateAccount;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;

    public AuthService(IMediator mediator, IEventBus eventBus)
    {
        _mediator = mediator;
        _eventBus = eventBus;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Username, request.Password, request.TypeLogin);
        var result = await _mediator.Send(command, cancellationToken);
        if (result is null)
        {
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
        }

        //await _eventBus.PublishAsync(
        //    new UserLoggedInIntegrationEvent(request.Username, request.TypeLogin),
        //    cancellationToken); // sau có consummer thì mới publish event này

        var accessTokenExpiresAt = GetAccessTokenExpiresAt(result.AccessToken);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(AuthTokenConstants.RefreshTokenExpireDays);

        return new LoginResponse(
            "Đăng nhập thành công.",
            result.AccessToken,
            result.RefreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        if (result is null)
        {
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
        }
        var accessTokenExpiresAt = GetAccessTokenExpiresAt(result.AccessToken);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(AuthTokenConstants.RefreshTokenExpireDays);

        return new RefreshTokenResponse(
            "Refresh token thành công.",
            result.AccessToken,
            result.RefreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }

    private static DateTimeOffset GetAccessTokenExpiresAt(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken))
        {
            throw new InvalidOperationException("Access token is not a valid Microsoft JWT.");
        }

        var token = handler.ReadJwtToken(accessToken);
        return new DateTimeOffset(DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc));
    }

    public async Task<UserRegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand( request.Email, request.Username, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        if (result is null)
        {
            throw new UnauthorizedAccessException("Đăng ký thất bại.");
        }
        return new UserRegisterResponse("Đăng ký thành công.", result);
    }

    public async Task<UserLoggoutResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(refreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        if (result is null)
        {
            throw new UnauthorizedAccessException("Logout thất bại.");
        }
        return result;
    }

    public async Task<bool> ActivateAccountAsync(ActivateAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new ActivateAccountCommand(request.Email, request.Code);
        return await _mediator.Send(command, cancellationToken);
    }
}
