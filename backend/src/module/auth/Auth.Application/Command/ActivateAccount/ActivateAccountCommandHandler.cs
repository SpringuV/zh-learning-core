namespace Auth.Application.Command.ActivateAccount;

public class ActivateAccountCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, ILogger<ActivateAccountCommandHandler> logger) : IRequestHandler<ActivateAccountCommand, bool>
{
    //  inject IAuthSessionCache để cache thông tin phiên đăng nhập của user, có thể dùng để check nếu user đã đăng nhập rồi thì không cần active lại
    // sau có thể dùng redis để cache thông tin phiên đăng nhập của user, và khi active account thì sẽ xóa cache
    //  đó đi để yêu cầu user phải đăng nhập lại, đảm bảo rằng chỉ có user đã active mới có thể đăng nhập thành công
    private readonly IIdentityService _identityService = identityService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<ActivateAccountCommandHandler> _logger = logger;

    public async Task<bool> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async () =>
            {
                return await _identityService.ActivateUserAsync(request.Email, request.Code, cancellationToken);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating account for {Email}", request.Email);
            return false; // trả về false để controller có thể trả về 400 Bad Request hoặc 500 Internal Server Error tùy vào loại lỗi
        }
    }
}
