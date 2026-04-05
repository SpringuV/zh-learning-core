using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.EventHandlers.Users;
using Search.Application.Queries.Users.Search;
using Search.Contracts.Interfaces;
using Search.Infrastructure.Services;

namespace Search.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // add mediator handlers from Search.Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(UserSearchQueries).Assembly));

        // Register Elasticsearch client (assuming it's added in API)
        // services.AddElasticsearchClient("elastic-hanzi"); // If not in API, add here

        // Register user-specific search service
        services.AddScoped<IUserSearchProjector, UserSearchQueriesServices>();
        services.AddScoped<IUserSearchQueriesServices, UserSearchQueriesServices>();

        // Register event handlers
        services.AddScoped<IIntegrationEventHandler<Auth.Contracts.IntegrationEvents.UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
    }
}