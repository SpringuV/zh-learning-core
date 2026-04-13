using HanziAnhVu.Shared.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HanziAnhVu.Shared.Application;

public static class MediatRPipelineExtensions
{
    public static IServiceCollection AddMediatRPipelineBehaviors(this IServiceCollection services)
    {
        // Registration order matters: validation -> caching -> logging -> handler.
        services.TryAddEnumerable(
            // dấu <,> để đăng ký generic pipeline behavior cho tất cả các request/response types.
            ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(
            ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>)));
        services.TryAddEnumerable(
            ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)));

        return services;
    }
}