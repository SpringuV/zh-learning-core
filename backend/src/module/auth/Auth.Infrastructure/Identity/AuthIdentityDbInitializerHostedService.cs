using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            var outboxDbContext = scope.ServiceProvider.GetRequiredService<Outbox.OutboxMessageDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (outboxDbContext.Database.IsNpgsql())
            {
                var migrated = await MigrateWithRetryAsync(outboxDbContext.Database, cancellationToken);
                if (!migrated)
                {
                    _logger.LogError("Outbox migration failed after retries. Skipping auth initialization for this startup.");
                    return;
                }
            }

            await AuthIdentityDbContextSeed.SeedAsync(identityDbContext, userManager, roleManager);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth identity/outbox initialization failed. Startup will continue without seeding.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<bool> MigrateWithRetryAsync(DatabaseFacade database, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await database.MigrateAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                if (attempt >= maxAttempts)
                {
                    _logger.LogError(ex,
                        "Database migration failed after {MaxAttempts} attempts.",
                        maxAttempts);
                    return false;
                }

                _logger.LogWarning(ex,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds}s.",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        return false;
    }
}
