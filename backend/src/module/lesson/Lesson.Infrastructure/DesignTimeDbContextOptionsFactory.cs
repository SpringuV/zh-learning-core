namespace Lesson.Infrastructure;
// file này lo phần cấu hình DbContext khi chạy lệnh migration, vì lúc này chưa có dependency injection để inject DbContext vào
internal static class DesignTimeDbContextOptionsFactory
{
    public static DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
    {
        var basePath = Directory.GetCurrentDirectory();

        // Lesson.Infrastructure does not contain appsettings.json locally.
        // When migrations are executed from this project, fall back to the API project folder.
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {   
            basePath = Path.GetFullPath(Path.Combine(basePath, "..", "src", "hanzi-anhvu-hsk"));
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("LessonDbConnection")
            ?? throw new Exception("Missing connection string");

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return optionsBuilder.Options;
    }
}
