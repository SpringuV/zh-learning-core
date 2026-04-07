using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities;

public enum StatusEnrollment
{
    Pending,
    Confirmed,
    Cancelled
}

public class ClassroomEnrollmentAggregate : BaseAggregateRoot
{
    public Guid EnrollmentId { get; private set; }
    public Guid ClassroomId { get; private set; }
    public Guid StudentId { get; private set; } // Liên kết đến UserId của học viên, soft link
    public Guid PaymentId { get; private set; } // Liên kết đến giao dịch thanh toán , soft link
    public DateTime EnrolledAt { get; private set; }
    public StatusEnrollment Status { get; private set; } = StatusEnrollment.Pending;

    public static ClassroomEnrollmentAggregate Create(Guid classroomId, Guid studentId, Guid paymentId)
    {
        if (classroomId == Guid.Empty) throw new ArgumentException("ClassroomId không được để trống.", nameof(classroomId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId không được để trống.", nameof(studentId));
        if (paymentId == Guid.Empty) throw new ArgumentException("PaymentId không được để trống.", nameof(paymentId));

        var enrollment = new ClassroomEnrollmentAggregate
        {
            EnrollmentId = Guid.CreateVersion7(),
            ClassroomId = classroomId,
            StudentId = studentId,
            PaymentId = paymentId,
            EnrolledAt = DateTime.UtcNow
        };
        
        return enrollment;
    }
}