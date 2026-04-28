namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TopicProgressConfiguration : IEntityTypeConfiguration<TopicProgressAggregate>
{
    public void Configure(EntityTypeBuilder<TopicProgressAggregate> builder)
    {
        builder.ToTable("TopicProgress");
        // Key configuration
        builder.HasKey(tp => tp.TopicProgressId);
        
        // Required properties
        builder.Property(tp => tp.UserId).IsRequired();
        builder.Property(tp => tp.TopicId).IsRequired();
        builder.Property(tp => tp.TotalAttempts).IsRequired();
        builder.Property(tp => tp.TotalCorrect).IsRequired();
        builder.Property(tp => tp.TotalAnswered).IsRequired();
        builder.Property(tp => tp.TotalWrong).IsRequired();
        builder.Property(tp => tp.TotalScore).IsRequired();
        builder.Property(tp => tp.CreatedAt).IsRequired();
        builder.Property(tp => tp.UpdatedAt).IsRequired();
        
        // Optional properties
        builder.Property(tp => tp.AccuracyRate).IsRequired(false);
        builder.Property(tp => tp.LastPracticedAt).IsRequired(false);
        
        // Cross-module: UserId is soft reference to users table (no FK constraint)
        
        // Same-module: TopicId is FK to topic
        builder.HasOne<TopicAggregate>()
            .WithMany()
            .HasForeignKey(tp => tp.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(tp => new { tp.UserId, tp.TopicId });
        
        // Other indexes
        builder.HasIndex(tp => tp.UserId);
        builder.HasIndex(tp => tp.TopicId);
    }
}
