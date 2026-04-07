using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities.Assignment;

/// <summary>
/// AssignmentExercise - Child entity of AssignmentAggregate
/// Represents exercises included in an assignment
/// Used for all assignment types (AllClass, Individual, Group)
/// </summary>
public class AssignmentExercise : Entity
{
    public Guid AssignmentId { get; private set; }
    public Guid ExerciseId { get; private set; } // Soft ref → exercise.id (cross-module)
    public int OrderIndex { get; private set; }

    public static AssignmentExercise Create(Guid assignmentId, Guid exerciseId, int orderIndex)
    {
        if (assignmentId == Guid.Empty)
            throw new ArgumentException("AssignmentId cannot be empty.", nameof(assignmentId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex cannot be negative.", nameof(orderIndex));

        return new AssignmentExercise
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignmentId,
            ExerciseId = exerciseId,
            OrderIndex = orderIndex
        };
    }
}

