namespace Classroom.Infrastructure.Config;

public class AssignmentRecippientConfiguration : IEntityTypeConfiguration<AssignmentRecipient>
{
    public void Configure(EntityTypeBuilder<AssignmentRecipient> builder)
    {
        builder.ToTable("AssignmentRecipients");
        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.AssignmentId).IsRequired();
        builder.Property(ar => ar.StudentId).IsRequired(); // Soft reference to Users
    }
}