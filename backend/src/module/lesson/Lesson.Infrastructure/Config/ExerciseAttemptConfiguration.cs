namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExerciseAttemptConfiguration : IEntityTypeConfiguration<ExerciseAttemptAggregate>
{
    public void Configure(EntityTypeBuilder<ExerciseAttemptAggregate> builder)
    {
        builder.ToTable("ExerciseAttempts");
        // Key configuration
        builder.HasKey(e => e.AttemptId);
        
        // Required properties
        builder.Property(e => e.ExerciseId).IsRequired();
        builder.Property(e => e.SessionId).IsRequired();
        builder.Property(e => e.Answer).IsRequired();
        builder.Property(e => e.SkillType).IsRequired();
        builder.Property(e => e.Score).IsRequired();
        builder.Property(e => e.IsCorrect).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        
        // Optional properties
        builder.Property(e => e.AiFeedback).IsRequired(false);
        
        // Relationship: ExerciseAttempt belongs to UserTopicExerciseSession (same-module)
        builder.HasOne<UserTopicExerciseSessionAggregate>()
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ExerciseAggregate>()
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
        // Cross-module: ExerciseId is soft reference to exercise table (no FK constraint)
        
        // Indexes
        builder.HasIndex(e => e.SessionId);
        builder.HasIndex(e => e.ExerciseId);
        builder.HasIndex(e => new { e.SessionId, e.ExerciseId });
    }
}
