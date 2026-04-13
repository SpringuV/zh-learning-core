namespace Lesson.Contracts.DTOs;

#region Course DTOs
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

#endregion

#region Topic DTOs
public sealed record CreateTopicResponseDTO(Guid TopicId);

public sealed record PublishTopicResponseDTO(
    Guid TopicId,
    DateTime UpdatedAt
);

#endregion