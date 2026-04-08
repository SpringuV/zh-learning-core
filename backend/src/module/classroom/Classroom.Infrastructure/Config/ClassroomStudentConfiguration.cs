namespace Classroom.Infrastructure.Config;

public class ClassroomStudentConfiguration : IEntityTypeConfiguration<ClassroomStudentAggregate>
{
    public void Configure(EntityTypeBuilder<ClassroomStudentAggregate> builder)
    {
        builder.ToTable("ClassroomStudents");
        // indexes
        builder.HasIndex(cs => cs.ClassroomId);
        builder.HasIndex(cs => cs.StudentId);
        builder.HasIndex(cs => new { cs.ClassroomId, cs.StudentId }).IsUnique(); // Ensure a student can only be added once per classroom
        // REQUIRED properties
        builder.HasKey(cs => cs.ClassroomStudentId);
        builder.Property(cs => cs.ClassroomStudentId).IsRequired();
        builder.Property(cs => cs.ClassroomId).IsRequired();          // FK to Classrooms
        builder.Property(cs => cs.StudentId).IsRequired();            // Soft ref to Users
        builder.Property(cs => cs.AddedBy).IsRequired();              // UserId who added student (usually TeacherId)
        builder.Property(cs => cs.JoinedAt).IsRequired();
        builder.Property(cs => cs.UpdatedAt).IsRequired();
        builder.Property(cs => cs.Status)
            .IsRequired()
            .HasConversion<string>();                                // Store enum as string (Active, Removed)
    }
}