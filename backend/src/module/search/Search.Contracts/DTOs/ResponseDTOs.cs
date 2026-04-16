using HanziAnhVu.Shared.Domain;

namespace Search.Contracts.DTOs;

#region Assignment Search DTOs
public sealed record AssignmentIndexResponse(
    Guid AssignmentId, 
    DateTime IndexedAt);

/// <summary>
/// Denormalized DTO for assignment search results (read-optimized)
/// </summary>
public sealed record AssignmentSearchItemResponse(
    string Id,
    Guid ClassroomId,
    Guid TeacherId,
    string Title,
    string Description,
    string AssignmentType,  // AllClass, Individual
    string SkillFocus,      // Reading, Writing, Listening, Speaking
    DateTime DueDate,
    bool IsPublished,
    int ExerciseCount,
    int RecipientCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record UserIndexResponse(Guid UserId, DateTime IndexedAt);

/// <summary>
/// Response after patching assignment document in Elasticsearch
/// </summary>
public sealed record AssignmentSearchPatchDocumentResponse(
    Guid? Id,
    string? Title = null,
    string? Description = null,
    string? SkillFocus = null,
    DateTime? DueDate = null,
    bool? IsPublished = null,
    int? ExerciseCount = null,
    int? RecipientCount = null,
    DateTime? UpdatedAt = null);
#endregion
#region User Search DTOs
public sealed record UserSearchPatchDocumentResponse(
    Guid? Id,
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? CurrentLevel = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchItemResponse(
	Guid Id,
	string Email,
	string Username,
	string? PhoneNumber,
	bool IsActive,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	int CurrentLevel);

public sealed record UserSearchResponse (
    string Id,
    string Email,
    string Username,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CurrentLevel,
    string? AvatarUrl,
    DateTime? LastLogin
);
#endregion
#region Course Search DTOs
public sealed record CourseIndexResponse(
    Guid CourseId,
    DateTime CreatedAt);

public sealed record CourseSearchItemAdminResponse(
    Guid Id,
    string Title,
    int HskLevel,
    int OrderIndex,
    long TotalTopics,
    long TotalStudentsEnrolled,
    bool IsPublished,
    string Slug,
    DateTime CreatedAt,
    DateTime UpdatedAt);
#endregion

#region  Topic Search DTOs

public sealed record TopicIndexResponse(
    Guid TopicId,
    DateTime CreatedAt);
public sealed record TopicSearchDetailResponse(
    Guid Id,
    string Title,
    string Slug,
    int OrderIndex,
    string TopicType,
    int? ExamYear,
    string? ExamCode,
    long EstimatedTimeMinutes,
    string Description,
    bool IsPublished,
    long TotalExercises,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TopicSearchItemAdminResponse(
    Guid Id,
    string Title,
    int OrderIndex,
    string TopicType,
    int? ExamYear,
    string? ExamCode,
    bool IsPublished,
    long TotalExercises,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CourseMetadataForTopicAdminResponse(
    Guid Id,
    string Title,
    int HskLevel,
    bool IsPublished,
    string Slug,
    long TotalTopics,
    long TotalStudentsEnrolled);

public sealed record TopicSearchWithCourseMetadataResponse(
    CourseMetadataForTopicAdminResponse? ParentMetadata,
    long Total,
    IReadOnlyList<TopicSearchItemAdminResponse> Items,
    bool HasNextPage,
    string NextCursor);
#endregion

#region Exercise Search DTOs
public sealed record ExerciseIndexResponse(
    Guid ExerciseId,
    DateTime CreatedAt);

public sealed record TopicMetadataForExerciseAdminResponse(
    Guid Id,
    string Title,
    long EstimatedTimeMinutes,
    bool IsPublished,
    string TopicType,
    int? ExamYear,
    string? ExamCode,
    string Slug,
    long TotalExercises);
public sealed record ExerciseSearchDetailResponse(
    Guid ExerciseId,
    string Question,
    string ExerciseType,
    string SkillType,
    string Difficulty,
    string Context,
    string Description,
    IReadOnlyList<ExerciseOption> Options,
    string CorrectAnswer,
    bool IsPublished,
    int OrderIndex,
    string? AudioUrl,
    string? ImageUrl,
    string? Explanation,
    string Slug,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
public sealed record ExerciseSearchWithTopicMetadataResponse(
    TopicMetadataForExerciseAdminResponse? ParentMetadata,
    long Total,
    IReadOnlyList<ExerciseSearchItemAdminResponse> Items,
    bool HasNextPage,
    string NextCursor);

public sealed record ExerciseSearchItemAdminResponse(
    Guid ExerciseId,
    string Question,
    string ExerciseType,
    string SkillType,
    string Difficulty,
    string Context,
    int OrderIndex,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
#endregion