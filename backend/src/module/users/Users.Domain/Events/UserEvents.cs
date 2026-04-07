using HanziAnhVu.Shared.Domain;

namespace Users.Domain.Events;

public record StudentEnrolledInCourseEvent(
    Guid StudentId,
    Guid CourseId,
    DateTime UpdatedAt
): BaseDomainEvent;