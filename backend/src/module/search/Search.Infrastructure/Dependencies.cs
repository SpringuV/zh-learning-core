using Search.Application.EventHandlers.Lesson.Topic;
using Search.Infrastructure.Queries.Lesson.Search.Validator;

namespace Search.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services, IHostApplicationBuilder host)
    {
        // add mediator handlers from Search.Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(UserSearchQueries).Assembly));

        // Register validators for MediatR ValidationBehavior.
        services.AddTransient<IValidator<TopicSearchAdminQueries>, TopicSearchAdminQueriesValidator>();
        services.AddTransient<IValidator<CourseSearchAdminQueries>, CourseSearchAdminQueriesValidator>();

        // Register Elasticsearch client (assuming it's added in API)
        host.AddElasticsearchClient("elastic-hanzi"); // If not in API, add here

        // Register user-specific search service
        services.AddScoped<IUserSearchQueriesServices, UserSearchQueriesServices>();
        
        // Register course-specific search service
        services.AddScoped<ICourseSearchQueriesService, CourseSearchQueriesServices>();

        // Register topic-specific search service
        services.AddScoped<ITopicSearchQueriesService, TopicSearchQueriesService>();

        // Register event handlers
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IIntegrationEventHandler<UserProfileUpdatedIntegrationEvent>, UserProfileUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<CourseCreatedIntegrationEvent>, CourseCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<TopicCreatedIntegrationEvent>, TopicCreatedEventHandler>();
    }
}