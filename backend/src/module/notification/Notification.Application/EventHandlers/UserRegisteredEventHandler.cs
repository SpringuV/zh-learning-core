using Auth.Contracts.IntegrationEvents;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Options;
using Notification.Application.Config;
using Notification.Application.Interfaces;

namespace Notification.Application.EventHandlers;

public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IEmailService _emailService;
    private readonly MailSettings _mailSettings;

    public UserRegisteredEventHandler(IEmailService emailService, IOptions<MailSettings> mailSettings)
    {
        _emailService = emailService;
        _mailSettings = mailSettings.Value;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default!)
    {

        // Send activation email
        await _emailService.SendEmailAsync(
            toEmail: @event.Email,
            subject: "Kích hoạt tài khoản",
            model: new { UserName = @event.Username, ActivationCode = @event.CodeActivate, ActivationLink = @event.ActivationLink },
            templateName: "ActivateAccountTemplate.cshtml"
        );
    }
}