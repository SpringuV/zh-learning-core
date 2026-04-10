
namespace Auth.Application.Command.Login;

// Irequest handler là nơi xử lý logic chính của command, có thể gọi service, repo, unit of work,... để thực hiện công việc
// login command sẽ là đầu vào dto logi
public class LoginUserCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, ITokenClaimService tokenClaimService, ILogger<LoginUserCommandHandler> logger) : IRequestHandler<LoginUserCommand, TokenResult?>
{
    private readonly ILogger<LoginUserCommandHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenClaimService _tokenClaimService = tokenClaimService;

    public async Task<TokenResult?> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async () =>
            {
                // 1 lần query DB: validate + load roles → truyền thẳng vào GetTokenAsync
                // Tránh FindByName lần 2 bên trong GetTokenAsync
                var result = await _identityService.ValidateCredentialsAsync(command.Username, command.Password, command.TypeLogin);
                return result is not null ? await _tokenClaimService.GetTokenAsync(result, cancellationToken) : null;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user {Username}", command.Username);
            return null; // trả về null để controller có thể trả về 401 Unauthorized
        }
    }

}
