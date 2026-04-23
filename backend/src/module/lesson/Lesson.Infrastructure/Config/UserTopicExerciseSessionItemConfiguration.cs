namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserTopicExerciseSessionItemConfiguration : IEntityTypeConfiguration<UserTopicExerciseSessionItem>
{
    public void Configure(EntityTypeBuilder<UserTopicExerciseSessionItem> builder)
    {
        builder.ToTable("UserTopicExerciseSessionItems");

        builder.HasKey(i => i.SessionItemId);

        builder.Property(i => i.SessionId).IsRequired();
        builder.Property(i => i.ExerciseId).IsRequired();
        builder.Property(i => i.SequenceNo).IsRequired();
        builder.Property(i => i.OrderIndex).IsRequired();
        builder.Property(i => i.Status).IsRequired();
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();

        builder.Property(i => i.AttemptId).IsRequired(false);
        builder.Property(i => i.ViewedAt).IsRequired(false);
        builder.Property(i => i.AnsweredAt).IsRequired(false);

        builder.HasIndex(i => i.SessionId);
        builder.HasIndex(i => new { i.SessionId, i.SequenceNo }).IsUnique();
        builder.HasIndex(i => new { i.SessionId, i.OrderIndex }).IsUnique();
        builder.HasIndex(i => i.ExerciseId);
    }
}