namespace Lesson.Infrastructure.Config;

public class CourseConfiguration : IEntityTypeConfiguration<CourseAggregate>
{
    public void Configure(EntityTypeBuilder<CourseAggregate> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.CourseId);
        builder.HasIndex(c => c.CourseId).IsUnique();
        builder.HasIndex(c => c.OrderIndex).IsUnique(); // đảm bảo không có 2 khóa học nào có cùng order index
        builder.Property(c => c.Title).IsRequired();
        builder.Property(c => c.Description).IsRequired(false);
        builder.Property(c => c.Slug).IsRequired();
        builder.Property(c => c.HskLevel).IsRequired();
        builder.Property(c => c.IsPublished).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
        builder.Property(c => c.OrderIndex).IsRequired();
        builder.Property(c => c.TotalStudentsEnrolled).IsRequired();
        builder.Property(c => c.TotalTopics).IsRequired();
    }
}
