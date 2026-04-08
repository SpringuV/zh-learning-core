namespace Classroom.Infrastructure.Config;

public class AssignmentConfiguration : IEntityTypeConfiguration<AssignmentAggregate>
{
    public void Configure(EntityTypeBuilder<AssignmentAggregate> builder)
    {
        builder.ToTable("Assignments");

        // Key configuration
        builder.HasKey(a => a.AssignmentId);
        builder.Property(a => a.AssignmentId).IsRequired();

        // Required properties
        builder.Property(a => a.ClassroomId).IsRequired();
        builder.Property(a => a.TeacherId).IsRequired();
        builder.Property(a => a.Title).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.Description).IsRequired(false).HasMaxLength(3000);
        builder.Property(a => a.AssignmentType).IsRequired();
        builder.Property(a => a.SkillFocus).IsRequired();
        builder.Property(a => a.DueDate).IsRequired();
        builder.Property(a => a.IsTimedAssignment).IsRequired().HasDefaultValue(false);     // ⏱️ Timed assignment?
        builder.Property(a => a.DurationMinutes).IsRequired(false);                         // ⏱️ Duration in minutes
        builder.Property(a => a.IsPublished).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // Child entities - AssignmentExercise
        builder.HasMany<AssignmentExercise>()
            .WithOne() // không có navigation property trong AssignmentExercise trỏ về Assignment, nên để trống
            .HasForeignKey("AssignmentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Child entities - AssignmentRecipient
        builder.HasMany<AssignmentRecipient>()
            .WithOne() // không có navigation property trong AssignmentRecipient trỏ về Assignment, nên để trống    
            .HasForeignKey("AssignmentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.ClassroomId);
        builder.HasIndex(a => a.TeacherId);
        builder.HasIndex(a => new {a.ClassroomId, a.TeacherId});
        builder.HasIndex(a => new { a.ClassroomId, a.IsPublished });
        builder.HasIndex(a => a.DueDate);
    }
}
