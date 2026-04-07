using Classroom.Domain.Entities.Assignment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        builder.Property(a => a.IsPublished).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // Child entities - AssignmentExercise
        builder.HasMany<AssignmentExercise>()
            .WithOne()
            .HasForeignKey("AssignmentId")
            .OnDelete(DeleteBehavior.Cascade);

        // Child entities - AssignmentRecipient
        builder.HasMany<AssignmentRecipient>()
            .WithOne()
            .HasForeignKey("AssignmentId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.ClassroomId);
        builder.HasIndex(a => a.TeacherId);
        builder.HasIndex(a => new {a.ClassroomId, a.TeacherId});
        builder.HasIndex(a => new { a.ClassroomId, a.IsPublished });
        builder.HasIndex(a => a.DueDate);
    }
}
