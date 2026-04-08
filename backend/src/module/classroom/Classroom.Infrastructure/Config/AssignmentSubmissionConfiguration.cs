namespace Classroom.Infrastructure.Config;

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmissionAggregate>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmissionAggregate> builder)
    {
        builder.ToTable("AssignmentSubmissions");

        // Key configuration
        builder.HasKey(a => a.SubmissionId);
        // Indexes
        builder.HasIndex(a => a.AssignmentId);
        builder.HasIndex(a => a.StudentId);
        builder.HasIndex(a => new { a.AssignmentId, a.StudentId });
        builder.HasIndex(a => a.DeadlineAt);  //  For auto-finalize queries
        
        // REQUIRED properties - Always have values
        builder.Property(a => a.SubmissionId).IsRequired();
        builder.Property(a => a.AssignmentId).IsRequired();  // Foreign key
        builder.Property(a => a.StudentId).IsRequired();      // Soft reference to Users
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>(); // hasConversion là  để lưu enum dưới dạng string trong database, giúp dễ đọc và tránh lỗi khi enum thay đổi
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // OPTIONAL properties - Set during lifecycle
        builder.Property(a => a.SelectedDurationMinutes).IsRequired(false);  // Set on Start()
        builder.Property(a => a.StartedAt).IsRequired(false);               // Set on Start()
        builder.Property(a => a.DeadlineAt).IsRequired(false);              // Calculated on Start()
        builder.Property(a => a.SubmittedAt).IsRequired(false);             // Set on Submit()
        builder.Property(a => a.FinalizedAt).IsRequired(false);             // Set on Finalize()
        builder.Property(a => a.GradedAt).IsRequired(false);                // Set on Grade()
        builder.Property(a => a.TotalScore).IsRequired(false);              // Set on Grade()
        builder.Property(a => a.TeacherFeedback)
            .IsRequired(false)
            .HasMaxLength(3000);                                    // Set on Grade()
    }
}