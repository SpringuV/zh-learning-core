namespace Users.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // DB cho users module
        var connectionString = configuration.GetConnectionString("UsersDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for UsersDbConnection. Set it in appsettings.json or environment variables.");

        // Configure EF Core với PostgreSQL
        services.AddDbContextPool<UserModuleDbContext>(options =>
            options.UseNpgsql(connectionString),
            poolSize: 128);

        // Outbox writer để enqueue events vào database
        services.AddScoped<IOutboxWriter, UserOutboxMessageWriter>();

        // repositories, nếu có (ví dụ: IUserRepository)
        services.AddScoped<IUserRepository, UserRepository>();

        // Unit of Work pattern
        services.AddScoped<IUnitOfWork, EfUnitOfWork<UserModuleDbContext>>();

        // Hosted services để initialize database và process outbox events
        services.AddHostedService<UserModuleDbInitializerHostedService>();
        services.AddHostedService<UserOutboxDispatcherWorker>();
    }
}
