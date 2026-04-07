namespace Auth.Application.Command.ActivateAccount;

public class ActivateAccountCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork) : IRequestHandler<ActivateAccountCommand, bool>
{
    //  inject IAuthSessionCache để cache thông tin phiên đăng nhập của user, có thể dùng để check nếu user đã đăng nhập rồi thì không cần active lại
    // sau có thể dùng redis để cache thông tin phiên đăng nhập của user, và khi active account thì sẽ xóa cache
    //  đó đi để yêu cầu user phải đăng nhập lại, đảm bảo rằng chỉ có user đã active mới có thể đăng nhập thành công
    private readonly IIdentityService _identityService = identityService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SaveChangeAsync(async () =>
        {
            return await _identityService.ActivateUserAsync(request.Email, request.Code, cancellationToken);
        }, cancellationToken);
    }
}
