namespace Classroom.Infrastructure.Config;

public class ClassroomConfiguration : IEntityTypeConfiguration<ClassroomAggregate>
{
    public void Configure(EntityTypeBuilder<ClassroomAggregate> builder)
    {
        builder.ToTable("Classrooms");

        // REQUIRED properties
        builder.HasKey(c => c.ClassroomId);
        // indexes
        builder.HasIndex(c => c.TeacherId);
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.ClassroomStatus);

        builder.Property(c => c.ClassroomId).IsRequired();
        builder.Property(c => c.TeacherId).IsRequired();              // Soft reference to Users
        builder.Property(c => c.Title).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.Description).IsRequired(false).HasMaxLength(3000);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
        builder.Property(c => c.HskLevel).IsRequired();               // 1-6
        builder.Property(c => c.ScheduleInfo).IsRequired(false).HasMaxLength(500);
        builder.Property(c => c.Price).IsRequired();
        builder.Property(c => c.PriceCurrency)
            .IsRequired()
            .HasConversion<string>();                                // Store enum as string
        builder.Property(c => c.ClassroomStatus)
            .IsRequired()
            .HasConversion<string>();                                // Store Status enum as string
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        // OPTIONAL properties - Set during lifecycle
        builder.Property(c => c.StartDate).IsRequired(false);
        builder.Property(c => c.EndDate).IsRequired(false);

        // Collection: StudentIds (many students in classroom)
        builder.Property(c => c.StudentIds)
            .HasConversion(
                v => string.Join(",", v),                            // Serialize to CSV, csv is a simple way to store list of Guids as string in DB
                v => v.Split(",", StringSplitOptions.RemoveEmptyEntries) // Deserialize back to List<Guid>
                    .Select(Guid.Parse)
                    .ToList())
            .HasMaxLength(5000); // Arbitrary max length for CSV string, adjust as needed
    }
}