using Classroom.Domain.Entities.Assignment;
using Classroom.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Infrastructure;

public class ClassroomDbContext : DbContext
{
    public ClassroomDbContext(DbContextOptions<ClassroomDbContext> options)
        : base(options)
    {
    }

    public DbSet<AssignmentAggregate> Assignments { get; set; }
    public DbSet<AssignmentSubmissionAggregate> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        // TODO: Add AssignmentSubmissionConfiguration, SubmissionAnswerConfiguration when needed
    }
}
