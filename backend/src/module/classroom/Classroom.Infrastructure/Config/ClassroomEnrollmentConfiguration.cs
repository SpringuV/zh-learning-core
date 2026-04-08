using Classroom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Classroom.Infrastructure.Config;

public class ClassroomEnrollmentConfiguration : IEntityTypeConfiguration<ClassroomEnrollmentAggregate>
{
    public void Configure(EntityTypeBuilder<ClassroomEnrollmentAggregate> builder)
    {
        builder.ToTable("ClassroomEnrollments");
        // index for efficient lookup of enrollment by student and classroom
        builder.HasIndex(e => new { e.StudentId, e.ClassroomId }).IsUnique();
        builder.HasIndex(e => e.ClassroomId);
        builder.HasIndex(e => e.StudentId);
        // REQUIRED properties
        builder.HasKey(e => e.EnrollmentId);
        builder.Property(e => e.EnrollmentId).IsRequired();
        builder.Property(e => e.ClassroomId).IsRequired();            // FK to Classrooms
        builder.Property(e => e.StudentId).IsRequired();              // Soft ref to Users
        builder.Property(e => e.EnrolledAt).IsRequired();
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();                                // Store enum as string (Pending, Confirmed, Cancelled)

        // OPTIONAL properties - Set on lifecycle
        builder.Property(e => e.PaymentId).IsRequired(false);         // Only set when ConfirmEnrollment(paymentId)
    }
}