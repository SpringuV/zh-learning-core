namespace Auth.Application.Command.Register;

// logic thuc thi khi register user, sẽ được gọi bởi controller
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityService _identityService;
    private readonly IPublisher _publisher; 
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SaveChangeAsync(async () =>
        {
            // IdentityService sẽ đảm nhận việc dùng UserManager của ASP.NET Core Identity 
            // để hash password và tạo user vào DB.
            var userId = await _identityService.RegisterUserAsync(
                request.Email,
                request.UserName,
                request.Password,
                cancellationToken);

            await _publisher.Publish(new AuthUserCreatedDomainEvent(userId, request.Email, request.UserName), cancellationToken);

            return userId;
        }, cancellationToken);
    }
}