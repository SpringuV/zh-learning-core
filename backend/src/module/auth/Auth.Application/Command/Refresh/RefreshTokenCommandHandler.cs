namespace Auth.Application.Command.Refresh;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResult?> 
    // cú pháp của MediatR: "đây là handler xử lý command RefreshTokenCommand và trả về TokenResult"
{
    private readonly ITokenClaimService _tokenClaimService;
    private readonly IUnitOfWork _unitOfWork;
    public RefreshTokenCommandHandler(ITokenClaimService tokenClaimService, IUnitOfWork unitOfWork)
    {
        _tokenClaimService = tokenClaimService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResult?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
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
}
