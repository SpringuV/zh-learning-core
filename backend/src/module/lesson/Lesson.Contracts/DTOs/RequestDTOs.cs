namespace Lesson.Contracts.DTOs;

using HanziAnhVu.Shared.Domain;

#region Course DTOs
public sealed record CreateCourseRequestDTO(
    string Title,
    string Description,
    int HskLevel,
    string Slug
);

public sealed record CourseReorderRequestDTO(
    List<Guid> OrderedCourseIds
);

#endregion

#region Topic DTOs
public sealed record TopicCreateRequestDTO(
    Guid CourseId,
    string Title,
    string Description,
    string TopicType,
    int EstimatedTimeMinutes,
    int? ExamYear = null,
    string? ExamCode = null
);

#endregion

#region Exercise DTOs
public sealed record ExerciseCreateRequestDTO(
    Guid TopicId,
    string Question,
    string Description,
    ExerciseType ExerciseType,
    ExerciseDifficulty Difficulty,
    SkillType SkillType,
    ExerciseContext ExerciseContext,
    List<ExerciseOptionDTO>? Options, // Nullable để cho các loại bài tập không yêu cầu option
    string CorrectAnswer, // Có thể là text hoặc option id tùy loại bài tập
    string Explanation,
    string? AudioUrl = null,
    string? ImageUrl = null
);

public sealed record ExerciseOptionDTO(
    string Id,
    string Text
);
#endregion