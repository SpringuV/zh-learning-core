using Auth.Application.DomainEventsInternal;
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
            var result = await _identityService.ResendMailActivateAsync(request.Email, cancellationToken);

            if (result is null)
            {
                // có thể log lại thông tin này để theo dõi
                return false; // hoặc throw new NotFoundException("User not found") nếu muốn rõ ràng hơn
            }

            var activationBaseUrl = _configuration["AppSettings:ActivationBaseUrl"];
            // đường link này sẽ có mã active và email của người dùng luôn
            // escape data string sẽ làm rối
            var activationLink = $"{activationBaseUrl}?email={Uri.EscapeDataString(request.Email)}&code={result.ActivationCode}";
            var resendLink = $"{activationBaseUrl}/resend?email={Uri.EscapeDataString(request.Email)}";
            // publish domain event để các service khác có thể subscribe
            // và thực hiện các logic liên quan đến user mới được tạo,
            // ví dụ như gửi email chào mừng, tạo profile mặc định, v.v.

            await _publisher.Publish(new AuthUserMailResentEvent(result.Email, result.ActivationCode, result.ExpiredActivation, activationLink, resendLink));
            return true;
        }, cancellationToken); 
    }
}
