using Auth.Domain.Exceptions;

namespace Auth.Application.Command.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        public ChangePasswordCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ChangePasswordCommand cmd, CancellationToken cancellationToken)
        {
            return await _unitOfWork.SaveChangeAsync(async ()=>
            {
                var result = await _identityService.ChangeUserPasswordAsync(cmd.UserId, cmd.NewPassword, cmd.OldPassword, cancellationToken);
                if (!result)
                {
                    throw new AuthDomainException("Change password failed");
                }
                return result;
            });
        }
    }
}
