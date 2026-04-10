using Microsoft.Extensions.Configuration;
namespace Auth.Application.Command.ResendMailActivate;

public class ResendMailActivateHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IConfiguration configuration, IPublisher publisher, ILogger<ResendMailActivateHandler> logger) : IRequestHandler<ResendMailActivateCommand, bool>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublisher _publisher = publisher;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ResendMailActivateHandler> _logger = logger;

    public async Task<bool> Handle(ResendMailActivateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.SaveChangeAsync(async ()=>
            {
                ResendMailActivationResponse result;
                switch (request.TypeUsername)
                {
                    case "Email":
                        // có thể validate email ở đây nếu muốn
                        result = await _identityService.ResendMailActivateAsync(request.Account, cancellationToken);
                        break;
                    case "Phone":
                        // có thể validate phone ở đây nếu muốn
                        var response = await _identityService.GetEmailUserByPhoneAsync(request.Account, cancellationToken);
                        if (response is null || string.IsNullOrEmpty(response.Email))
                        {
                            return false;
                        }
                        result = await _identityService.ResendMailActivateAsync(response.Email, cancellationToken);
                        break;
                    case "Username":
                        // có thể validate username ở đây nếu muốn
                        var emailResponse = await _identityService.GetEmailUserByUsernameAsync(request.Account, cancellationToken);
                        if (emailResponse is null || string.IsNullOrEmpty(emailResponse.Email))
                        {
                            return false;
                        }
                        result = await _identityService.ResendMailActivateAsync(emailResponse.Email, cancellationToken);
                        break;
                    default:
                        throw new ArgumentException("Invalid type username");
                }
            
                if (result is null) return false;

                var activationBaseUrl = _configuration["AppSettings:ActivationBaseUrl"];
                // đường link này sẽ có mã active và email của người dùng luôn
                // escape data string sẽ làm rối
                var activationLink = $"{activationBaseUrl}?account={Uri.EscapeDataString(result.Email)}&code={result.ActivationCode}";
                var resendLink = $"{activationBaseUrl}/resend?account={Uri.EscapeDataString(result.Email)}";
                // publish domain event để các service khác có thể subscribe
                // và thực hiện các logic liên quan đến user mới được tạo,
                // ví dụ như gửi email chào mừng, tạo profile mặc định, v.v.

                await _publisher.Publish(new AuthUserMailResentEvent(result.Email, result.ActivationCode, result.ExpiredActivation, activationLink, resendLink));
                return true;
            }, cancellationToken); 
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid resend mail activate request for account {Account}", request.Account);
            return false; // trả về false để controller có thể trả về 400 Bad Request với message chi tiết
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error resending mail activate for account {Account}", request.Account);
            return false; // trả về false để controller có thể trả về 500 Internal Server Error
        }
    }
}
