using Auth.Contracts.IntegrationEvents;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Notification.Application.Interfaces;

namespace Notification.Application.EventHandlers;

public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IEmailService _emailService;

    public UserRegisteredEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default!)
    {
        // Send activation email
        await _emailService.SendEmailAsync(
            toEmail: @event.Email,
            subject: "Kích hoạt tài khoản",
            model: new { UserName = @event.Username, @event.ActivationLink, @event.ResendLink },
            templateName: "ActivateAccountTemplate.cshtml"
        );
    }
}