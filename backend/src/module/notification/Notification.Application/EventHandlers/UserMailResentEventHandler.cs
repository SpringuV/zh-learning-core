using Auth.Contracts.IntegrationEvents;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Notification.Application.Interfaces;

namespace Notification.Application.EventHandlers;

public class UserMailResentEventHandler : IIntegrationEventHandler<UserMailResentIntegrationEvent>
{
    private readonly IEmailService _emailService;

    public UserMailResentEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(UserMailResentIntegrationEvent @event, CancellationToken ct = default!)
    {
        await _emailService.SendEmailAsync(
                toEmail: @event.Email,
                subject: "Kích hoạt tài khoản",
                model: new { UserName = @event.Email, @event.ActivationLink, @event.ResendLink},
                templateName: "ActivateAccountTemplate.cshtml"
            );
    }
}
