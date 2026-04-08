namespace Classroom.Infrastructure.Config;

public class SubmissionAnswerConfiguration : IEntityTypeConfiguration<SubmissionAnswer>
{
    public void Configure(EntityTypeBuilder<SubmissionAnswer> builder)
    {
        builder.ToTable("SubmissionAnswers");
        builder.HasIndex(sa => sa.SubmissionId);
        builder.HasIndex(sa => sa.ExerciseId);
        builder.HasIndex(sa => new { sa.SubmissionId, sa.ExerciseId });
        
        builder.HasKey(sa => sa.Id);
        builder.Property(sa => sa.SubmissionId).IsRequired();
        builder.Property(sa => sa.ExerciseId).IsRequired(); // Soft reference to Users
        builder.Property(sa => sa.Answer).IsRequired().HasMaxLength(2000);
        builder.Property(sa => sa.IsCorrect).IsRequired();
        builder.Property(sa => sa.Feedback).IsRequired(false).HasMaxLength(3000);
        builder.Property(sa => sa.Score).IsRequired(false);
        builder.Property(sa => sa.CreatedAt).IsRequired();
    }
}