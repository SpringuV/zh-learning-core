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

        // Register exercise-specific search service
        services.AddScoped<IExerciseSearchQueriesService, ExerciseSearchQueriesService>();

        RegisterIntegrationEventHandlers(services);
    }

    private static void RegisterIntegrationEventHandlers(IServiceCollection services)
    {
        var openGenericHandlerType = typeof(IIntegrationEventHandler<>);
        var handlersAssembly = typeof(ExerciseCreatedEventHandler).Assembly;
        // Find all classes that implement IIntegrationEventHandler<T> and register them
        // dù ở bất kỳ namespace nào trong assembly, miễn là implement IIntegrationEventHandler<T> thì đều được đăng ký vào DI container với lifetime
        var registrations = handlersAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(implementationType => implementationType
                .GetInterfaces()
                .Where(serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == openGenericHandlerType)
                .Select(serviceType => new { serviceType, implementationType }));

        foreach (var registration in registrations)
        {
            services.AddScoped(registration.serviceType, registration.implementationType);
        }
    }
}