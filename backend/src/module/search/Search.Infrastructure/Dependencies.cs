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
        services.AddScoped<IUserSearchQueriesServices, UserSearchQueriesServices>();

        // Register event handlers
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
    }
}