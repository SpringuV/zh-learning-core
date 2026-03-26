using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.EventHandlers;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        // Register event handlers
        services.AddScoped<IIntegrationEventHandler<Auth.Contracts.IntegrationEvents.UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
    }
}