using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace Users.Infrastructure.Persistence;

/// <summary>
/// IHostedService để khởi tạo database và setup outbox trigger cho Users module khi ứng dụng khởi động.
/// Tương tự như AuthIdentityDbInitializerHostedService nhưng dành cho Users module.
/// Chạy tự động khi app start, thiết lập migration, seed dữ liệu, và config trigger PostgreSQL notify.
/// Trên production, nên dùng migration script thay vì seed dữ liệu.
/// </summary>
public sealed class UserModuleDbInitializerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<UserModuleDbInitializerHostedService> logger) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<UserModuleDbInitializerHostedService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Tạo scope để resolve scoped services (DbContext, etc.)
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userDbContext = scope.ServiceProvider.GetRequiredService<UserModuleDbContext>();

            // Nếu dùng PostgreSQL, chạy migration với retry logic
            if (userDbContext.Database.IsNpgsql())
            {
                // Migration với retry để xử lý trường hợp database chưa sẵn sàng
                var migrated = await userDbContext.Database.MigrateWithRetryAsync(
                    _logger,
                    cancellationToken,
                    maxAttempts: 10,
                    retryDelay: TimeSpan.FromSeconds(2));

                if (!migrated)
                {
                    _logger.LogError("User module migration failed after retries. Skipping initialization for this startup.");
                    return;
                }

                // Đảm bảo trigger cho outbox table - dùng channel riêng "user_outbox_channel"
                // để tách biệt với Auth module channel
                await userDbContext.Database.EnsureOutboxNotifyTriggerAsync(
                    tableName: "OutboxMessagesUsersModule",
                    channelName: "user_outbox_channel",
                    functionName: "notify_user_outbox_change",
                    triggerName: "user_outbox_change_trigger",
                    cancellationToken: cancellationToken);

                _logger.LogInformation("User module database and outbox trigger initialized successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User module database initialization failed. Startup will continue without seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
