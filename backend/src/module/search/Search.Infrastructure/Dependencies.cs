using Lesson.Contracts;
using Microsoft.Extensions.Hosting;
using Search.Application.EventHandlers.Lesson.Course;

namespace Search.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services, IHostApplicationBuilder host)
    {
        // add mediator handlers from Search.Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(UserSearchQueries).Assembly));

        // Register Elasticsearch client (assuming it's added in API)
        host.AddElasticsearchClient("elastic-hanzi"); // If not in API, add here

        // Register user-specific search service
        services.AddScoped<IUserSearchQueriesServices, UserSearchQueriesServices>();
        
        // Register course-specific search service
        services.AddScoped<ICourseSearchQueriesService, CourseSearchQueriesServices>();

        // Register event handlers
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IIntegrationEventHandler<UserProfileUpdatedIntegrationEvent>, UserProfileUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<CourseCreatedIntegrationEvent>, CourseCreatedEventHandler>();
    }
}