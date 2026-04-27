using HanziAnhVu.Shared.Domain;

namespace Search.Contracts.DTOs;

public sealed record PaginationResponse(
    int Page,
    int PageSize,
    long Total);
#region ExerciseSession

public record ExerciseSessionPracticeItemWithoutAnswerResponse(
    Guid ExerciseId,
    string Question,
    string ExerciseType,
    string SkillType,
    string Difficulty,
    string Description,
    int OrderIndex,
    IReadOnlyList<ExerciseOption> Options,
    string? AudioUrl,
    string? ImageUrl,
    string Slug
);

public record CountinueLearningExerciseDTO: ExerciseSessionPracticeItemWithoutAnswerResponse
{
    public CountinueLearningExerciseDTO(
        Guid ExerciseId,
        string Question,
        string ExerciseType,
        string SkillType,
        string Difficulty,
        string Description,
        int OrderIndex,
        IReadOnlyList<ExerciseOption> Options,
        string? AudioUrl,
        string? ImageUrl,
        string Slug
    ) : base(ExerciseId, 
            Question, 
            ExerciseType, 
            SkillType, 
            Difficulty, 
            Description, 
            OrderIndex, 
            Options, 
            AudioUrl, ImageUrl,
            Slug)
    {
        
    }
}
// chỉ đánh dấu là đã nộp bài, không trả về kết quả đúng sai ở đây vì có thể sẽ có nhiều loại bài tập khác nhau với các cách thức trả lời khác nhau (text, optionId, file upload...)
public record SubmitAnswerResponseDTO(
    Guid ExerciseId,
    int SequenceNo,
    string Status,
    DateTime AnsweredAt
);
public record BaseExerciseLearningSessionItemsDTO(Guid SessionItemId,
	Guid ExerciseId,
	int SequenceNo,
	int OrderIndex,
	Guid? AttemptId,
	string Status,
	DateTime? ViewedAt,
	DateTime? AnsweredAt);

public record CountinueLearningSessionItemDTO: BaseExerciseLearningSessionItemsDTO
{
    public CountinueLearningSessionItemDTO(
        Guid SessionItemId,
        Guid ExerciseId,
        int SequenceNo,
        int OrderIndex,
        Guid? AttemptId,
        string Status,
        DateTime? ViewedAt,
        DateTime? AnsweredAt) : base(SessionItemId, ExerciseId, SequenceNo, OrderIndex, AttemptId, Status, ViewedAt, AnsweredAt)
    {
    }
}


public record CountinueLearningResponseDTO(
	Guid SessionId,
	int TotalExercises,
	int CurrentSequenceNo,
	IReadOnlyList<CountinueLearningSessionItemDTO> SessionItems,
	CountinueLearningExerciseDTO FirstExercise
);

public record ExerciseSessionItemsSnapshotResponse(
    Guid SessionId,
    int TotalExercises,
	int CurrentSequenceNo,
    IReadOnlyList<BaseExerciseLearningSessionItemsDTO> SessionItems
);

#endregion

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
public sealed record CourseSearchForDashboardClientResponse(
    Guid Id,
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    long TotalTopics,
    long TotalStudentsEnrolled,
    string Slug
);
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

public enum StatusTopicForDashboardClient
{
    NotStarted,
    Abandoned,
    InProgress,
    Completed
}

public sealed record TopicSearchForDashboardClientResponse(
    Guid Id,
    string Title,
    string Slug,
    int OrderIndex,
    string TopicType,
    int? ExamYear,
    string? ExamCode,
    long EstimatedTimeMinutes,
    string Description,
    StatusTopicForDashboardClient Status,
    long TotalExercises);

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
    IReadOnlyList<TopicSearchItemAdminResponse> Items,
    PaginationResponse Pagination);
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
    IReadOnlyList<ExerciseSearchItemAdminResponse> Items,
    PaginationResponse Pagination);

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