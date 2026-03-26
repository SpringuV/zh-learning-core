namespace Auth.Application.Command.Register;

// logic thuc thi khi register user, sẽ được gọi bởi controller, thực thi và trả về cho api
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid?>
{
    private readonly IIdentityService _identityService;
    private readonly IPublisher _publisher; 
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IPublisher publisher,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _publisher = publisher; // in process, dùng cho domain event nội bộ, không giao tiếp với các service,
                                // dùng trong cùng bounded context để publish domain event,
                                // còn outbox sẽ dùng để giao tiếp với các service khác thông qua integration event
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid?> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.SaveChangeAsync(async () =>
        {
            // IdentityService sẽ đảm nhận việc dùng UserManager của ASP.NET Core Identity 
            // để hash password và tạo user vào DB.
            var registerResponse = await _identityService.RegisterUserAsync(
                request.Email,
                request.UserName,
                request.Password,
                cancellationToken);

            if(registerResponse is null)
            {
                // Nếu tạo user thất bại, có thể throw exception hoặc trả về null tùy theo thiết kế.
                return Guid.Empty;
            }

            // publish domain event để các service khác có thể subscribe
            // và thực hiện các logic liên quan đến user mới được tạo,
            // ví dụ như gửi email chào mừng, tạo profile mặc định, v.v.
            await _publisher.Publish(new AuthUserCreatedDomainEvent(registerResponse.UserId, request.Email, request.UserName, registerResponse.CreatedAt, registerResponse.ActivateCode), cancellationToken);

            // Enqueue integration event vào outbox để đảm bảo tính nhất quán khi giao tiếp với các service khác.
            await _outboxWriter.EnqueueAsync(
                new UserRegisteredIntegrationEvent(registerResponse.UserId, request.Email, request.UserName, registerResponse.CreatedAt, registerResponse.ActivateCode),
                cancellationToken);

            // trả về cho api thì chỉ cần mỗi user id
            return registerResponse.UserId;
        }, cancellationToken);
    }
}