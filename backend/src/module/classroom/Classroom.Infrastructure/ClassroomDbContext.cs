using Classroom.Infrastructure.Outbox;
using Shared.Infrastructure.Outbox;
namespace Classroom.Infrastructure;

public class ClassroomDbContext(DbContextOptions<ClassroomDbContext> options) : DbContext(options)
{
    public DbSet<AssignmentAggregate> Assignments { get; set; } = null!;
    public DbSet<AssignmentSubmissionAggregate> AssignmentSubmission { get; set; } = null!;
    public DbSet<ClassroomStudentAggregate> ClassroomStudents { get; set; } = null!;
    public DbSet<ClassroomEnrollmentAggregate> ClassroomEnrollments { get; set; } = null!;
    public DbSet<ClassroomAggregate> Classrooms { get; set; } = null!;
    public DbSet<ClassroomModuleOutboxMessage> ClassroomOutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new AssignmentSubmissionConfiguration());
        modelBuilder.ApplyConfiguration(new ClassroomStudentConfiguration());
        modelBuilder.ApplyConfiguration(new ClassroomEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new ClassroomConfiguration());
        // TODO: Add SubmissionAnswerConfiguration when needed
        modelBuilder.Entity<ClassroomModuleOutboxMessage>(entity =>
        {
            entity.ConfigureOutboxMessage("OutboxMessagesClassroomModule");
        });
    }
}
