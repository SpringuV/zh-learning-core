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

        // auto scan and regist all eventhandler in application assembly NOTIFICATION.APPLICATION
        var applicationAssembly = typeof(UserRegisteredEventHandler).Assembly;
        var handlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
            services.AddScoped(interfaceType, handlerType);
        }
    }
}