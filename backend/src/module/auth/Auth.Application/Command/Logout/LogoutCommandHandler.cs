using Auth.Contracts.DTOs;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Command.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, UserLoggoutResponse>
{
    private readonly ITokenClaimService _tokenClaimService;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(ITokenClaimService tokenClaimService, ILogger<LogoutCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _tokenClaimService = tokenClaimService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserLoggoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // revoke the refresh token

        return await _unitOfWork.SaveChangeAsync( async()=>
        {
            var result = await _tokenClaimService.RevokeAsync(request.RefreshToken, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Đăng xuất thành công");
                return new UserLoggoutResponse("Đăng xuất thành công !");
            }
            else
            {
               _logger.LogError("Đăng xuất thất bại !");
                return new UserLoggoutResponse("Đăng xuất thất bại !");
            }
        }, cancellationToken);
    }
}
