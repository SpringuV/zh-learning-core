namespace Lesson.Contracts.DTOs;

public sealed record CreateCourseResponseDTO(Guid CourseId);

public sealed record UpdateCourseResponseDTO(
    Guid CourseId,
    string Title,
    string Description,
    int OrderIndex,
    string Slug,
    DateTime UpdatedAt
);

public sealed record PublishCourseResponseDTO(
    Guid CourseId,
    DateTime UpdatedAt
);