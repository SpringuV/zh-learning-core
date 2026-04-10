namespace Auth.Application.Command.UpdateProfile;

public class UpdateProfileHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IPublisher publisher, ILogger<UpdateProfileHandler> logger) : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly ILogger<UpdateProfileHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async () =>
            {
                var result = await _identityService.UpdateProfileAsync(request.UserId, request.NewPhoneNumber, request.NewAvatarUrl);
                if (result != null)
                {
                    await _publisher.Publish(new AuthUserProfileUpdatedEvent(result.UserId, result.NewPhoneNumber ?? string.Empty, result.NewAvatarUrl ?? string.Empty, result.UpdatedAt), cancellationToken);
                }
                return result != null; // nếu khác null thì == true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
            return false; // trả về false để controller có thể trả về 500 Internal Server Error
        }
    }
}