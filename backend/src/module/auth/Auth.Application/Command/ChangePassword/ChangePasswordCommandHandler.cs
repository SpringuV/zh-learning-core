using Auth.Domain.Exceptions;

namespace Auth.Application.Command.ChangePassword
{
    public class ChangePasswordCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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
