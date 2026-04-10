using Auth.Contracts.DTOs;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Command.Logout;

public class LogoutCommandHandler(ITokenClaimService tokenClaimService, ILogger<LogoutCommandHandler> logger, IUnitOfWork unitOfWork, IIdentityService identityService) : IRequestHandler<LogoutCommand, UserLoggoutResponse>
{
    private readonly ITokenClaimService _tokenClaimService = tokenClaimService;
    private readonly ILogger<LogoutCommandHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IIdentityService _identityService = identityService;

    public async Task<UserLoggoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // revoke the refresh token

        try
        {
            var result = await _tokenClaimService.RevokeAsync(request.RefreshToken, cancellationToken);
            if (result.HasValue)
            {
                // revoke token thành công mới update last logout time, tránh trường hợp token đã bị revoke trước đó rồi mà vẫn update last logout time
                await _identityService.UpdateLastLogoutTimeAsync(result.Value, cancellationToken);
                _logger.LogInformation("Đăng xuất thành công");
                return new UserLoggoutResponse("Đăng xuất thành công !");
            }
            _logger.LogError("Đăng xuất thất bại !");
            return new UserLoggoutResponse("Đăng xuất thất bại !");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while logging out");
            return new UserLoggoutResponse("Đăng xuất thất bại !");
        }
    }
}
