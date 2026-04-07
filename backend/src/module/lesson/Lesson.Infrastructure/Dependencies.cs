namespace Lesson.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Register DbContext
        // DB cho users module
        var connectionString = configuration.GetConnectionString("LessonDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for LessonDbConnection. Set it in appsettings.json or environment variables.");

        // Configure EF Core với PostgreSQL
        services.AddDbContextPool<LessonDbContext>(options =>
            options.UseNpgsql(connectionString),
            poolSize: 128);
        
        services.AddScoped<IOutboxWriter, LessonOutboxMessageWriter>();
        services.AddHostedService<LessonModuleDbInitializerHostedService>(); // Hosted service để khởi tạo database và setup trigger outbox khi ứng dụng khởi động
        services.AddHostedService<LessonOutboxDispatcherWorker>(); // Hosted service để lắng nghe và dispatch outbox events từ PostgreSQL
        services.AddScoped<IUnitOfWork, EfUnitOfWork<LessonDbContext>>(); // Unit of Work pattern cho Lesson module
        services.AddScoped<IExerciseRepository, ExerciseRepository>(); // Repository cho Lesson module
        services.AddScoped<ITopicRepository, TopicRepository>(); // Repository cho Lesson module
        services.AddScoped<ITopicProgressRepository, TopicProgressRepository>(); // Repository cho Lesson module
        services.AddScoped<IUserTopicExerciseSessionRepository, UserTopicExerciseSessionRepository>(); // Repository cho Lesson module
        services.AddScoped<IExerciseAttemptRepository, ExerciseAttemptRepository>(); // Repository cho Lesson module
        services.AddScoped<ICourseRepository, CourseRepository>(); // Repository cho Lesson module
    }
}