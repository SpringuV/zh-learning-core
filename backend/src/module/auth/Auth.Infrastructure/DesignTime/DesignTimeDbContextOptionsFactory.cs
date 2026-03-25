using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.DesignTime;

internal static class DesignTimeDbContextOptionsFactory
{
    public static DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
    {
        var basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("AuthIdentityDbConnection")
            ?? throw new Exception("Missing connection string");

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return optionsBuilder.Options;
    }
}
