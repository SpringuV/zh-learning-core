using Auth.Infrastructure.Services;
using HanziAnhVu.Shared.Infrastructure;
using Auth.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // DB cho auth/identity
        var connectionString = configuration.GetConnectionString("AuthIdentityDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for AuthIdentityDbConnection. Set it in appsettings.json or environment variables.");

        // Configure EF Core logging for detailed timing
        services.AddDbContextPool<AuthIdentityDbContext>(options =>
            options.UseNpgsql(connectionString),
            poolSize: 128);

        // Required for Identity token providers (reset/change email/password tokens)
        services.AddDataProtection();

        // ASP.NET Identity
        services.AddIdentityCore<AuthApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthIdentityDbContext>()
            .AddDefaultTokenProviders();

        // MediatR handlers trong Auth.Application
        // regis tër tất cả handlers (RequestHandler, NotificationHandler, ...) trong assembly của Auth.Application.Services.AuthService
        // assemble là sẽ tìm tất cả các class implement interface MediatR.IRequestHandler<,>
        // hoặc MediatR.INotificationHandler<> trong assembly đó và đăng ký chúng vào DI container
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Auth.Application.Services.AuthService).Assembly));

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.POLICY_ADMIN_ONLY, policy => policy.RequireRole(Constants.ADMINISTRATORS));
            options.AddPolicy(Constants.POLICY_USER_ONLY, policy => policy.RequireRole(Constants.USERS));
            options.AddPolicy(Constants.POLICY_ADMIN_OR_MANAGER, policy => policy.RequireRole(Constants.ADMINISTRATORS, "Manager"));
        });

        // Auth services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenClaimService, TokenClaimService>();
        services.AddScoped<IOutboxWriter, OutboxMessageWriter>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork<AuthIdentityDbContext>>();
        services.AddScoped<IAuthService, Application.Services.AuthService>();

        // Seed/migrate khi app start (dev/local)
        services.AddHostedService<AuthIdentityDbInitializerHostedService>();
        services.AddHostedService<AuthOutboxDispatcherServiceWorker>();
    }
}
