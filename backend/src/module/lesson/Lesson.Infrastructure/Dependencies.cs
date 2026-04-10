namespace Lesson.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCourseHandler).Assembly));
        
        // Register DbContext
        // DB cho Lesson module
        var connectionString = configuration.GetConnectionString("LessonDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for LessonDbConnection. Set it in appsettings.json or environment variables.");

        // Configure EF Core với PostgreSQL
        services.AddDbContextPool<LessonDbContext>(options =>
            options.UseNpgsql(connectionString),
            poolSize: 128);
        
        // Register module-specific UnitOfWork để tránh bị ghi đè bởi module khác cùng dùng IUnitOfWork
        services.AddScoped<ILessonUnitOfWork, LessonUnitOfWork>();
        
        services.AddScoped<ILessonService, LessonService>(); // Service layer cho Lesson module
        services.AddScoped<ILessonOutboxWriter, LessonOutboxMessageWriter>();
        services.AddHostedService<LessonModuleDbInitializerHostedService>(); // Hosted service để khởi tạo database và setup trigger outbox khi ứng dụng khởi động
        services.AddHostedService<LessonOutboxDispatcherWorker>(); // Hosted service để lắng nghe và dispatch outbox events từ PostgreSQL
        services.AddScoped<IExerciseRepository, ExerciseRepository>(); // Repository cho Lesson module
        services.AddScoped<ITopicRepository, TopicRepository>(); // Repository cho Lesson module
        services.AddScoped<ITopicProgressRepository, TopicProgressRepository>(); // Repository cho Lesson module
        services.AddScoped<IUserTopicExerciseSessionRepository, UserTopicExerciseSessionRepository>(); // Repository cho Lesson module
        services.AddScoped<IExerciseAttemptRepository, ExerciseAttemptRepository>(); // Repository cho Lesson module
        services.AddScoped<ICourseRepository, CourseRepository>(); // Repository cho Lesson module
    }
}