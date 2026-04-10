namespace Auth.Application.Command.Register;

// logic thuc thi khi register user, sẽ được gọi bởi controller, thực thi và trả về cho api
public class RegisterUserCommandHandler(
    IIdentityService identityService,
    IPublisher publisher,   // in process, dùng cho domain event nội bộ, không giao tiếp với các service,
                            // dùng trong cùng bounded context để publish domain event,
                            // còn outbox sẽ dùng để giao tiếp với các service khác thông qua integration event
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ILogger<RegisterUserCommandHandler> logger) : IRequestHandler<RegisterUserCommand, Guid?>
{
    private readonly ILogger<RegisterUserCommandHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IPublisher _publisher = publisher; 
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Guid?> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async () =>
            {
                // 1. Validate input BEFORE transaction
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.UserName))
                {
                    throw new ArgumentException("Email and UserName are required");
                }

                // 2. Register user
                var registerResponse = await _identityService.RegisterUserAsync(
                    request.Email,
                    request.UserName,
                    request.Password,
                    cancellationToken) ?? throw new InvalidOperationException("Failed to create user");

                // 3. Build links
                var activationBaseUrl = _configuration["AppSettings:ActivationBaseUrl"]
                    ?? throw new InvalidOperationException("ActivationBaseUrl not configured");

                var activationLink = $"{activationBaseUrl}?account={Uri.EscapeDataString(request.Email)}&code={registerResponse.ActivateCode}";
                var resendLink = $"{activationBaseUrl}/resend?account={Uri.EscapeDataString(request.Email)}";

                // 4. Publish event
                await _publisher.Publish(
                    new AuthUserCreatedDomainEvent(
                        registerResponse.UserId,
                        request.Email,
                        request.UserName,
                        registerResponse.CreatedAt,
                        registerResponse.ActivateCode,
                        activationLink,
                        resendLink),
                    cancellationToken);

                return registerResponse.UserId;
            }, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid registration request for {Email}", request.Email);
            throw; // throw lại để controller có thể trả về 400 Bad Request với message chi tiết
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Registration logic failed for {Email}. Rolling back transaction.", request.Email);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during registration for {Email}. Rolling back transaction.", request.Email);
            throw new InvalidOperationException("Database error during registration", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}. Rolling back transaction.", request.Email);
            throw new InvalidOperationException("Registration failed due to unexpected error", ex);
        }
    }
}