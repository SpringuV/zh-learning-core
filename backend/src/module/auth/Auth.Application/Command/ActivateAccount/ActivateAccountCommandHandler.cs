namespace Auth.Application.Command.ActivateAccount;

public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateAccountCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SaveChangeAsync(async () =>
        {
            return await _identityService.ActivateUserAsync(request.Email, request.Code, cancellationToken);
        }, cancellationToken);
    }
}
