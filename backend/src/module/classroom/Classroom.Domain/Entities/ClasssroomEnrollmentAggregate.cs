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
    public Guid? PaymentId { get; private set; } // Liên kết đến giao dịch thanh toán , soft link (set khi ConfirmEnrollment)
    public DateTime EnrolledAt { get; private set; }
    public StatusEnrollment Status { get; private set; } = StatusEnrollment.Pending;

    public static ClassroomEnrollmentAggregate Create(Guid classroomId, Guid studentId)
    {
        if (classroomId == Guid.Empty) throw new ArgumentException("ClassroomId không được để trống.", nameof(classroomId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId không được để trống.", nameof(studentId));

        var enrollment = new ClassroomEnrollmentAggregate
        {
            EnrollmentId = Guid.CreateVersion7(),
            ClassroomId = classroomId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow,
            Status = StatusEnrollment.Pending
        };
        
        // Emit event when enrollment created
        enrollment.AddDomainEvent(new ClassroomEnrollmentCreatedEvent(
            enrollment.EnrollmentId,
            enrollment.ClassroomId,
            enrollment.StudentId,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt
        ));
        
        return enrollment;
    }

    public void CancelEnrollment()
    {
        if (Status != StatusEnrollment.Pending)
            throw new InvalidOperationException("Chỉ có thể hủy enrollment đang ở trạng thái Pending.");

        Status = StatusEnrollment.Cancelled;
        
        AddDomainEvent(new ClassroomEnrollmentCancelledEvent(
            EnrollmentId,
            ClassroomId,
            StudentId,
            Status.ToString(),
            DateTime.UtcNow
        ));
    }

    public void ConfirmEnrollment(Guid paymentId)
    {
        if (paymentId == Guid.Empty) throw new ArgumentException("PaymentId không được để trống.", nameof(paymentId));
        if (Status != StatusEnrollment.Pending)
            throw new InvalidOperationException("Chỉ có thể xác nhận enrollment đang ở trạng thái Pending.");

        PaymentId = paymentId;
        Status = StatusEnrollment.Confirmed;
        
        AddDomainEvent(new ClassroomEnrollmentConfirmedEvent(
            EnrollmentId,
            ClassroomId,
            StudentId,
            PaymentId,
            Status.ToString(),
            DateTime.UtcNow
        ));
    }
}
    