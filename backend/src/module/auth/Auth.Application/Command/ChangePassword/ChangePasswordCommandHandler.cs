namespace Auth.Application.Command.ChangePassword
{
    public class ChangePasswordCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, ILogger<ChangePasswordCommandHandler> logger) : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly ILogger<ChangePasswordCommandHandler> _logger = logger;
        private readonly IIdentityService _identityService = identityService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> Handle(ChangePasswordCommand cmd, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.SaveChangeAsync(async ()=>
                {
                    var result = await _identityService.ChangeUserPasswordAsync(cmd.UserId, cmd.NewPassword, cmd.OldPassword, cancellationToken);
                    if (!result)
                    {
                        throw new AuthDomainException("Change password failed");
                    }
                    return result;
                }, cancellationToken);
            }
            catch (AuthDomainException ex)
            {
                _logger.LogWarning(ex, "Failed to change password for user {UserId}: {Message}", cmd.UserId, ex.Message);
                return false; // trả về false để controller có thể trả về 400 Bad Request với message chi tiết
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error changing password for user {UserId}", cmd.UserId);
                return false; // trả về false để controller có thể trả về 500 Internal Server Error
            }
        }
    }
}
