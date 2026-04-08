namespace Lesson.Infrastructure.Config;

using Lesson.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TopicConfiguration : IEntityTypeConfiguration<TopicAggregate>
{
    public void Configure(EntityTypeBuilder<TopicAggregate> builder)
    {
        builder.ToTable("Topics");
        // Configure the Topic entity
        builder.HasKey(c => c.TopicId);
        builder.HasIndex(t => t.CourseId); // tạo index trên CourseId để truy vấn nhanh hơn
        builder.HasIndex(t => t.ExamCode); // tạo index trên ExamCode để truy vấn nhanh hơn
        builder.HasIndex(t => new { t.CourseId, t.OrderIndex }).IsUnique(); // đảm bảo mỗi topic trong cùng course có orderIndex khác nhau

        builder.Property(t => t.Title).IsRequired();
        builder.Property(t => t.Description).IsRequired(false);
        builder.Property(t => t.TopicType).IsRequired();
        builder.Property(t => t.Slug).IsRequired();
        builder.Property(t => t.EstimatedTimeMinutes).IsRequired();
        builder.Property(t => t.ExamYear).IsRequired(false);
        builder.Property(t => t.ExamCode).IsRequired(false);
        builder.Property(t => t.OrderIndex).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.Property(t => t.IsPublished).IsRequired();

        builder.HasOne<CourseAggregate>() // mỗi topic thuộc về 1 course
            .WithMany() // course có nhiều topic
            .HasForeignKey(t => t.CourseId) // khóa ngoại trong bảng topic trỏ về course
            .OnDelete(DeleteBehavior.Cascade); // khi xóa course thì xóa luôn các topic liên quan
    }
}
