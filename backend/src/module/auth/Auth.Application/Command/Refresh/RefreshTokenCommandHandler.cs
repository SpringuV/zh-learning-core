namespace Auth.Application.Command.Refresh;

public class RefreshTokenCommandHandler(ITokenClaimService tokenClaimService, IUnitOfWork unitOfWork, ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, TokenResult?> 
    // cú pháp của MediatR: "đây là handler xử lý command RefreshTokenCommand và trả về TokenResult"
{
    private readonly ILogger<RefreshTokenCommandHandler> _logger = logger;
    private readonly ITokenClaimService _tokenClaimService = tokenClaimService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TokenResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async () =>
            {
                var result = await _tokenClaimService.RefreshAsync(request.RefreshToken, cancellationToken);
                if (result != null)
                {
                    return result;
                }
                return null;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for refresh token: {RefreshToken}", request.RefreshToken);
            return null;
        }
    }
}
