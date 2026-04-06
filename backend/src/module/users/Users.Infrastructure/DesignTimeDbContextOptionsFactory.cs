using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Users.Infrastructure;

internal static class DesignTimeDbContextOptionsFactory
{
    public static DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
    {
        var basePath = Directory.GetCurrentDirectory();
        // Adjust basePath to point to the main project directory where appsettings.json is located
        // entry point to call it is path backend
        // basePath = Path.Combine(basePath, "..", "src", "hanzi-anhvu-hsk"); // this called startup project

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("UsersDbConnection")
            ?? throw new Exception("Missing connection string");

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return optionsBuilder.Options;
    }
}
