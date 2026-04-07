using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities.Assignment;

/// <summary>
/// AssignmentRecipient - Child entity of AssignmentAggregate
/// Represents specific students who receive an individual assignment
/// Used only when assignment_type = Individual
/// </summary>
public class AssignmentRecipient : Entity
{
    public Guid AssignmentId { get; private set; }
    public Guid StudentId { get; private set; } // Soft ref → users.id (cross-module)

    public static AssignmentRecipient Create(Guid assignmentId, Guid studentId)
    {
        if (assignmentId == Guid.Empty)
            throw new ArgumentException("AssignmentId cannot be empty.", nameof(assignmentId));
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));

        return new AssignmentRecipient
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignmentId,
            StudentId = studentId
        };
    }
}