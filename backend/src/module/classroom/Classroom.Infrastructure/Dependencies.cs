using Classroom.Infrastructure.Outbox;
using Classroom.Infrastructure.Persistence;
using Classroom.Infrastructure.Repository;
using HanziAnhVu.Shared.Application;
using HanziAnhVu.Shared.Infrastructure;

namespace Classroom.Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Register DbContext
        // DB cho Lesson module
        var connectionString = configuration.GetConnectionString("ClassroomDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for ClassroomDbConnection. Set it in appsettings.json or environment variables.");

        // Configure EF Core với PostgreSQL
        services.AddDbContextPool<ClassroomDbContext>(options =>
            options.UseNpgsql(connectionString),
            poolSize: 128);

        services.AddScoped<IOutboxWriter, ClassroomModuleMessageWriter>();
        services.AddHostedService<ClassroomModuleDbInitializerHostedService>(); // Hosted service để khởi
        services.AddHostedService<ClassroomModuleDispatcherWorker>(); // Hosted service để lắng nghe và dispatch outbox events từ PostgreSQL
        services.AddScoped<IUnitOfWork, EfUnitOfWork<ClassroomDbContext>>(); // Unit of Work pattern cho Classroom module
        services.AddScoped<IClassroomRepository, ClassroomRepository>(); // Repository cho Classroom module
        services.AddScoped<IClassroomStudentRepository, ClassroomStudentRepository>(); // Repository cho Classroom module
        services.AddScoped<IClassroomEnrollmentRepository, ClassroomEnrollmentRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>(); 
        services.AddScoped<IAssignmentSubmissionRepository, AssignmentSubmissionRepository>(); 
    }
}