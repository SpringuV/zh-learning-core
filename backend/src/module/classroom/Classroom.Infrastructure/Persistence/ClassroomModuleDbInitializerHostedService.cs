namespace Classroom.Infrastructure.Persistence;

/// <summary>
/// IHostedService để khởi tạo database và setup outbox trigger cho Lesson module khi ứng dụng khởi động.
/// Tương tự như AuthIdentityDbInitializerHostedService nhưng dành cho Lesson module.
/// Chạy tự động khi app start, thiết lập migration, seed dữ liệu, và config trigger PostgreSQL notify.
/// Trên production, nên dùng migration script thay vì seed dữ liệu.
/// </summary>
public sealed class ClassroomModuleDbInitializerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ClassroomModuleDbInitializerHostedService> logger) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ClassroomModuleDbInitializerHostedService> _logger = logger;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Tạo scope để resolve scoped services (DbContext, etc.)
            await using var scope = _scopeFactory.CreateAsyncScope();
            var classroomDbContext = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();
            // Nếu dùng PostgreSQL, chạy migration với retry logic
            if (classroomDbContext.Database.IsNpgsql())
            {
                var migrated = await classroomDbContext.Database.MigrateWithRetryAsync(
                    _logger,
                    cancellationToken,
                    maxAttempts: 10,
                    retryDelay: TimeSpan.FromSeconds(2));

                if (!migrated){
                    _logger.LogError("Classroom module migration failed after retries. Skipping initialization for this startup.");
                    return; 
                }
                // Đảm bảo trigger cho outbox table - dùng channel riêng "classroom_outbox_channel"
                // để tách biệt với Auth và Users module channel
                await classroomDbContext.Database.EnsureOutboxNotifyTriggerAsync(
                    tableName: "OutboxMessagesClassroomModule", // Tên bảng outbox riêng cho classroom module   
                    channelName: "classroom_outbox_channel", // Tên channel riêng cho classroom module
                    functionName: "notify_classroom_outbox_change",
                    triggerName: "classroom_outbox_change_trigger",
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Classroom module database and outbox trigger initialized successfully.");
            }

        } catch (Exception ex)
        {
            _logger.LogError(ex, "Classroom module database initialization failed. Startup will continue without seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}