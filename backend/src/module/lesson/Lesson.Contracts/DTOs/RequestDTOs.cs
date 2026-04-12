namespace Lesson.Contracts.DTOs;

public sealed record CreateCourseRequestDTO(
    string Title,
    string Description,
    int HskLevel,
    string Slug
);

public sealed record CreateTopicRequestDTO(
    string Title,
    string Description,
    string TopicType,
    int EstimatedTimeMinutes,
    int? ExamYear = null,
    string? ExamCode = null
);

public sealed record CourseReorderRequestDTO(
    List<Guid> OrderedCourseIds
);