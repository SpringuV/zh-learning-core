using Shared.Infrastructure;

namespace Lesson.Infrastructure.Persistence;

/// <summary>
/// IHostedService để khởi tạo database và setup outbox trigger cho Lesson module khi ứng dụng khởi động.
/// Tương tự như AuthIdentityDbInitializerHostedService nhưng dành cho Lesson module.
/// Chạy tự động khi app start, thiết lập migration, seed dữ liệu, và config trigger PostgreSQL notify.
/// Trên production, nên dùng migration script thay vì seed dữ liệu.
/// </summary>

public sealed class LessonModuleDbInitializerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<LessonModuleDbInitializerHostedService> logger) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<LessonModuleDbInitializerHostedService> _logger = logger;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Tạo scope để resolve scoped services (DbContext, etc.)
            await using var scope = _scopeFactory.CreateAsyncScope();
            var lessonDbContext = scope.ServiceProvider.GetRequiredService<LessonDbContext>();
            // Nếu dùng PostgreSQL, chạy migration với retry logic
            if (lessonDbContext.Database.IsNpgsql())
            {
                var migrated = await lessonDbContext.Database.MigrateWithRetryAsync(
                    _logger,
                    cancellationToken,
                    maxAttempts: 10,
                    retryDelay: TimeSpan.FromSeconds(2));

                if (!migrated){
                    _logger.LogError("Lesson module migration failed after retries. Skipping initialization for this startup.");
                    return; 
                }
                // Đảm bảo trigger cho outbox table - dùng channel riêng "lesson_outbox_channel"
                // để tách biệt với Auth và Users module channel
                await lessonDbContext.Database.EnsureOutboxNotifyTriggerAsync(
                    tableName: "LessonOutboxMessages",
                    channelName: "lesson_outbox_channel", // Tên channel riêng cho lesson module
                    functionName: "notify_lesson_outbox_change",
                    triggerName: "lesson_outbox_change_trigger",
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Lesson module database and outbox trigger initialized successfully.");
            }

        } catch (Exception ex)
        {
            _logger.LogError(ex, "Lesson module database initialization failed. Startup will continue without seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}