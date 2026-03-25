
namespace Auth.Application.Command.Login;

// Irequest handler là nơi xử lý logic chính của command, có thể gọi service, repo, unit of work,... để thực hiện công việc
// login command sẽ là đầu vào dto logi
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, TokenResult?>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenClaimService _tokenClaimService;

    public LoginUserCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, ITokenClaimService tokenClaimService)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _tokenClaimService = tokenClaimService;
    }

    public async Task<TokenResult?> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SaveChangeAsync(async () =>
        {
            // 1 lần query DB: validate + load roles → truyền thẳng vào GetTokenAsync
            // Tránh FindByName lần 2 bên trong GetTokenAsync
            var result = await _identityService.ValidateCredentialsAsync(command.Username, command.Password, command.TypeLogin);
            return result is not null ? await _tokenClaimService.GetTokenAsync(result, cancellationToken) : null;
        }, cancellationToken);
    }

}
