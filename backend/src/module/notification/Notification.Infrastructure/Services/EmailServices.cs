using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Notification.Application.Config;
using Notification.Application.Interfaces;
using RazorLight;

namespace Notification.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly MailSettings _settings;
    private readonly IRazorLightEngine _razorEngine;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
    {
        _settings = mailSettings.Value;
        _logger = logger;
        _razorEngine = new RazorLightEngineBuilder()
            // appdomain.current.base directory will point to the output directory of the project, so we can place our templates in a "Templates" folder there
            .UseFileSystemProject(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task SendEmailAsync(string toEmail, string subject, object model, string templateName)
    {
        var htmlBody = await _razorEngine.CompileRenderAsync(templateName, model);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.Name, _settings.EmailId));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        SecureSocketOptions options = _settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await client.ConnectAsync(_settings.Host, _settings.Port, options);
        await client.AuthenticateAsync(_settings.UserName, _settings.Password);
        _logger.LogInformation("Sending email to {Email} with subject {Subject}", toEmail, subject);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        _logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
    }
}
