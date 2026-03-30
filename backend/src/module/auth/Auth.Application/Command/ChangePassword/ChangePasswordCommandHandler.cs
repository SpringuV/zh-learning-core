using Auth.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

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

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.SaveChangeAsync(async ()=>
            {
                var result = await _identityService.ChangeUserPasswordAsync(request.UserId, request.NewPassword, request.OldPassword, cancellationToken);
                if (!result)
                {
                    throw new AuthDomainException("Change password failed");
                }
                return result;
            });
        }
    }
}
