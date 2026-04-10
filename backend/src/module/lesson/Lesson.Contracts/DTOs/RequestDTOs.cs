namespace Lesson.Contracts.DTOs;

public sealed record CreateCourseRequestDTO(
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    string Slug
);

public sealed record CreateTopicRequestDTO(
    string Title,
    string Description,
    string TopicType,
    int EstimatedTimeMinutes,
    int OrderIndex,
    int? ExamYear = null,
    string? ExamCode = null
);