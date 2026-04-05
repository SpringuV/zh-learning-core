namespace Auth.Application.Command.UpdateProfile;

public class UpdateProfileHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IPublisher publisher) : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
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
}