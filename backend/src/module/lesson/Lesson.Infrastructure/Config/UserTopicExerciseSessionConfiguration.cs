namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserTopicExerciseSessionConfiguration : IEntityTypeConfiguration<UserTopicExerciseSessionAggregate>
{
    public void Configure(EntityTypeBuilder<UserTopicExerciseSessionAggregate> builder)
    {
        builder.ToTable("UserTopicExerciseSessions");
        // Key configuration
        builder.HasKey(s => s.SessionId);
        
        // Required properties
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.TimeSpentSeconds).IsRequired();
        
        // Required FK: every exercise session belongs to a topic in the current domain model
        builder.Property(s => s.TopicId).IsRequired();
        builder.Property(s => s.TotalCorrect).IsRequired();
        builder.Property(s => s.TotalScore).IsRequired(false);
        builder.Property(s => s.ScoreListening).IsRequired();
        builder.Property(s => s.ScoreReading).IsRequired();
        builder.Property(s => s.ScoreWriting).IsRequired();
        builder.Property(s => s.TotalWrong).IsRequired();
        builder.Property(s => s.CompletedAt).IsRequired(false);
        builder.Property(s => s.CurrentSequenceNo).IsRequired();
        builder.Property(s => s.TotalExercises).IsRequired();
        
        // Cross-module: UserId is soft reference to users table (no FK constraint)
        // Same-module: TopicId is a required FK to topic
        
        builder.HasOne<TopicAggregate>()
            .WithMany()
            .HasForeignKey(s => s.TopicId)
            .OnDelete(DeleteBehavior.Cascade); // If session is associated with a topic, cascade delete when topic is deleted

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.TopicId);
        builder.HasIndex(s => new { s.UserId, s.Status });
        builder.HasIndex(s => s.StartedAt);

        builder.HasMany(s => s.SessionItems)
            .WithOne()
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
