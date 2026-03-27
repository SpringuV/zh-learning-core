using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure;

namespace Auth.Infrastructure.Identity;

// IHostedService để chạy code khởi tạo database và seed dữ liệu khi ứng dụng khởi động
// dùng cho AppHost trong aspire, sẽ tự động chạy StartAsync khi ứng dụng bắt đầu và StopAsync khi ứng dụng dừng
// trên production, thì không nên dùng cách này để seed dữ liệu, mà nên dùng migration hoặc script SQL để đảm bảo an toàn và kiểm soát tốt hơn
public sealed class AuthIdentityDbInitializerHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuthIdentityDbInitializerHostedService> _logger;

    public AuthIdentityDbInitializerHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuthIdentityDbInitializerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var identityDbContext = scope.ServiceProvider.GetRequiredService<AuthIdentityDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (identityDbContext.Database.IsNpgsql())
            {
                var migrated = await identityDbContext.Database.MigrateWithRetryAsync(
                    _logger,
                    cancellationToken,
                    maxAttempts: 10,
                    retryDelay: TimeSpan.FromSeconds(2));
                if (!migrated)
                {
                    _logger.LogError("Outbox migration failed after retries. Skipping auth initialization for this startup.");
                    return;
                }

                await identityDbContext.Database.EnsureOutboxNotifyTriggerAsync(
                    tableName: "OutboxMessages",
                    channelName: "outbox_channel",
                    cancellationToken: cancellationToken);
            }

            await AuthIdentityDbContextSeed.SeedAsync(identityDbContext, userManager, roleManager);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth identity/outbox initialization failed. Startup will continue without seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
