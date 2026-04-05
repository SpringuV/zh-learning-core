using Microsoft.Extensions.Configuration;
namespace Auth.Application.Command.ResendMailActivate;

public class ResendMailActivateHandler : IRequestHandler<ResendMailActivateCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly IConfiguration _configuration;

    public ResendMailActivateHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IConfiguration configuration, IPublisher publisher)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _publisher = publisher;
    }

    public async Task<bool> Handle(ResendMailActivateCommand request, CancellationToken cancellationToken)
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
}
