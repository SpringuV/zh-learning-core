namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExerciseConfiguration : IEntityTypeConfiguration<ExerciseAggregate>
{
    public void Configure(EntityTypeBuilder<ExerciseAggregate> builder)
    {
        builder.ToTable("Exercises");
        // Key configuration
        builder.HasKey(e => e.ExerciseId);
        builder.Property(e => e.ExerciseId).IsRequired();
        
        // Required properties
        builder.Property(e => e.TopicId).IsRequired();
        builder.Property(e => e.Question).IsRequired();
        builder.Property(e => e.ExerciseType).IsRequired();
        builder.Property(e => e.Slug).IsRequired();
        builder.Property(e => e.SkillType).IsRequired();
        builder.Property(e => e.Difficulty).IsRequired();
        builder.Property(e => e.Context).IsRequired().HasDefaultValue(ExerciseContext.Learning);
        builder.Property(e => e.OrderIndex).IsRequired();
        builder.Property(e => e.IsPublished).IsRequired();
        
        // Optional properties
        builder.Property(e => e.Description).IsRequired(false);
        builder.Property(e => e.CorrectAnswer).IsRequired(false);
        builder.Property(e => e.AudioUrl).IsRequired(false);
        builder.Property(e => e.ImageUrl).IsRequired(false);
        builder.Property(e => e.Explanation).IsRequired(false);
        
        // JSON conversion for Options (Value Object collection)
        var optionsProperty = builder.Property(e => e.Options)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }),
                v => System.Text.Json.JsonSerializer.Deserialize<List<ExerciseOption>>(v, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }) ?? new List<ExerciseOption>()
            )
            .IsRequired();
        
        // Set value comparer for change tracking
        optionsProperty.Metadata.SetValueComparer(
            new ValueComparer<IReadOnlyList<ExerciseOption>>(
                equalsExpression: (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                hashCodeExpression: c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                snapshotExpression: c => new List<ExerciseOption>(c)
            )
        );
        
        // Relationship: Exercise belongs to Topic (same-module)
        builder.HasOne<TopicAggregate>()
            .WithMany()
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(e => e.TopicId);
        builder.HasIndex(e => new { e.TopicId, e.OrderIndex });
    }
}
