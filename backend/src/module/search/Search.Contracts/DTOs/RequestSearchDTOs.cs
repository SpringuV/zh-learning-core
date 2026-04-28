using HanziAnhVu.Shared.Domain;

namespace Search.Contracts.DTOs;

#region Exercise Search DTOs

public sealed record ExerciseOptionIndexDTOs(string Id, string Text);
public sealed record ExerciseSearchIndexQueriesRequest(
    Guid ExerciseId,
    Guid TopicId,
    string Question,
    int OrderIndex,
    string Description,
    string CorrectAnswer,
    string ExerciseType,  // MultipleChoice, FillInTheBlank, TrueFalse, etc.
    string SkillType,     // Reading, Writing, Listening, Speaking
    string Difficulty,    // Easy, Medium, Hard
    string Context,       // Vocabulary, Grammar, Conversation, etc.
    string? AudioUrl,
    string? ImageUrl,
    string Slug,
    string? Explanation,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ExerciseOptionIndexDTOs> Options);
public sealed record ExerciseDeletedRequestDTO(
    Guid ExerciseId);
public sealed record ExercisePublishedRequestDTO(
    Guid ExerciseId,
    DateTime UpdatedAt);
public sealed record ExerciseUnPublishedRequestDTO(
    Guid ExerciseId,
    DateTime UpdatedAt);

public sealed record ExerciseReorderSearchRequestDTO(
    Guid TopicId,
    IReadOnlyList<Guid> OrderedExerciseIds,
    DateTime UpdatedAt);
public sealed record ExerciseTypeUpdatedRequestDTO(
    Guid ExerciseId,
    Guid TopicId,
    ExerciseType NewExerciseType,
    DateTime UpdatedAt);
public sealed record ExerciseSkillTypeUpdatedRequestDTO(
    Guid ExerciseId,
    Guid TopicId,
    SkillType NewSkillType,
    DateTime UpdatedAt);
public sealed record ExerciseContextUpdatedRequestDTO(
    Guid ExerciseId,
    ExerciseContext NewContext,
    DateTime UpdatedAt);
public sealed record ExerciseDescriptionUpdatedRequestDTO(
    Guid ExerciseId,
    string NewDescription,
    DateTime UpdatedAt);
public sealed record ExerciseQuestionUpdatedRequestDTO(
    Guid ExerciseId,
    string NewQuestion,
    string NewSlug,
    DateTime UpdatedAt);
public sealed record ExerciseCorrectAnswerUpdatedRequestDTO(
    Guid ExerciseId,
    string NewCorrectAnswer,
    DateTime UpdatedAt);
public sealed record ExerciseDifficultyUpdatedRequestDTO(
    Guid ExerciseId,
    ExerciseDifficulty NewDifficulty,
    DateTime UpdatedAt);
public sealed record ExerciseAudioUrlUpdatedRequestDTO(
    Guid ExerciseId,
    string NewAudioUrl,
    DateTime UpdatedAt);
public sealed record ExerciseImageUrlUpdatedRequestDTO(
    Guid ExerciseId,
    string NewImageUrl,
    DateTime UpdatedAt);
public sealed record ExerciseExplanationUpdatedRequestDTO(
    Guid ExerciseId,
    string NewExplanation,
    DateTime UpdatedAt);
public sealed record ExerciseOptionsUpdatedRequestDTO(
    Guid ExerciseId,
    IReadOnlyList<ExerciseOption> NewOptions,
    DateTime UpdatedAt);
public enum ExerciseSortBy
{
    CreatedAt,
    UpdatedAt,
    OrderIndex
}
public sealed record ExerciseSearchQueryRequest(
    Guid TopicId,
    string? Question = null,
    bool? IsPublished = null,
    string? SkillType = null,
    string? ExerciseType = null,
    string? Difficulty = null,
    string? Context = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    int Page = 1,
    ExerciseSortBy SortBy = ExerciseSortBy.CreatedAt,
    bool OrderByDescending = true);

public sealed record ExerciseAttemptBatchScoredRequestDTO(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    IReadOnlyList<ExerciseAttemptBatchScoredItemDTO> Attempts
);


public sealed record ExerciseSessionCompletedRequestDTO(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    ExerciseSessionStatus Status,
    int TotalExercises,
    int HskLevel,
    float TotalScore,
    int ScoreListening,
    int ScoreReading,
    int TotalCorrect,
    int TotalWrong,
    int TimeSpentSeconds,
    DateTime CompletedAt);
#endregion


#region Assignment Search DTOs
public sealed record AssignmentSearchIndexQueriesRequest(
    Guid Id,
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

public record UserSearchIndexQueriesRequest(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Query request for searching assignments
/// Keyset pagination using SearchAfterCreatedAt to avoid offset inefficiency
/// </summary>
public sealed record AssignmentSearchQueryRequest(
    Guid? ClassroomId = null,
    Guid? TeacherId = null,
    string? SkillFocus = null,  // Reading, Writing, Listening, Speaking
    bool? IsPublished = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SearchQuery = null,  // Full-text search on Title + Description
    int Take = 30,
    string? SearchAfterCreatedAt = null,  // Keyset pagination cursor (CreatedAt from last item)
    AssignmentSortBy SortBy = AssignmentSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null);

/// <summary>
/// Sort options for assignment search
/// </summary>
public enum AssignmentSortBy
{
    CreatedAt,
    UpdatedAt,
    DueDate,
    Title,
}

/// <summary>
/// Incremental update request for assignment in Elasticsearch
/// Used to sync partial changes without reindexing entire document
/// </summary>
public sealed record AssignmentSearchPatchDocumentRequest(
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
public sealed record UserSearchPatchDocumentRequest(
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? CurrentLevel = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchQueryRequest(
    string? Email = null,
    string? Username = null,
    bool? IsActive = null,
    string? PhoneNumber = null,
    int Take = 30,
    int Page = 1,
    UserSortBy SortBy = UserSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null);

public enum UserSortBy
{
    CreatedAt,
    UpdatedAt,
    CurrentLevel,
    Email,
    Username,
}
#endregion

#region Topic Search DTOs
public sealed record TopicDescriptionUpdatedRequestDTO(
    Guid CourseId,
    Guid TopicId,
    string NewDescription,
    DateTime UpdatedAt);
public sealed record TopicExamInfoUpdatedRequestDTO(
    Guid CourseId,
    Guid TopicId,
    int? NewExamYear,
    string NewExamCode,
    DateTime UpdatedAt);
public sealed record TopicDeletedRequestDTO(
    Guid TopicId);
public sealed record TopicTotalExercisePublishedUpdatedRequestDTO(
    Guid TopicId,
    int TotalExercisesPublished,
    DateTime UpdatedAt);
public sealed record TopicEstimatedTimeUpdatedRequestDTO(
    Guid CourseId,
    Guid TopicId,
    long NewEstimatedTimeMinutes,
    DateTime UpdatedAt);
public sealed record TopicTitleUpdatedRequestDTO(
    Guid CourseId,
    Guid TopicId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt);
public sealed record TopicReorderSearchRequestDTO(
    Guid CourseId,
    IReadOnlyList<Guid> OrderedTopicIds,
    DateTime UpdatedAt);

public sealed record TopicUnPublishedRequestDTO(
    Guid TopicId,
    DateTime UpdatedAt);
public sealed record TopicPublishedRequestDTO(
    Guid TopicId,
    DateTime UpdatedAt);
public sealed record TopicTotalExercisesUpdatedRequestDTO(
    Guid TopicId,
    long TotalExercises,
    DateTime UpdatedAt);
public sealed record TopicSearchIndexQueriesRequest(
    Guid TopicId,
    Guid CourseId,
    string Title,
    string Description,
    int OrderIndex,
    string TopicType,
    string Slug,
    long EstimatedTimeMinutes,
    int ExamYear,
    string ExamCode,
    bool IsPublished,
    long TotalExercises,
    int TotalExercisesPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt);
#endregion

#region Course Search DTOs
public sealed record CourseReorderSearchRequestDTO(
    List<Guid> OrderedCourseIds,
    DateTime UpdatedAt);
public sealed record CoursePublishedSearchRequestDTO(
    Guid CourseId,
    DateTime UpdatedAt);
public sealed record CourseUnPublishedSearchRequestDTO(
    Guid CourseId,
    DateTime UpdatedAt);
public sealed record CourseTotalTopicsUpdatedSearchRequestDTO(
    Guid CourseId,
    long TotalTopics,
    DateTime UpdatedAt);
public sealed record CourseTitleUpdatedSearchRequestDTO(
    Guid CourseId,
    string Title,
    string Slug,
    DateTime UpdatedAt);
public sealed record CourseDescriptionUpdatedSearchRequestDTO(
    Guid CourseId,
    string NewDescription,
    DateTime UpdatedAt);
public sealed record CourseDeletedSearchRequestDTO(
    Guid CourseId
);
public sealed record CourseHskLevelUpdatedSearchRequestDTO(
    Guid CourseId,
    int HskLevel,
    DateTime UpdatedAt);
public sealed record CourseTotalTopicsPublishedUpdatedSearchRequestDTO(
    Guid CourseId,
    long TotalTopicsPublished,
    DateTime UpdatedAt);

public sealed record CourseSearchIndexQueriesRequest(
    Guid CourseId,
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    int TotalTopicsPublished,
    string Slug,
    long TotalStudentsEnrolled,
    long TotalTopics,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt);
public enum CourseSortBy
{
    CreatedAt,
    UpdatedAt,
    Title,
    HskLevel,
    OrderIndex,
    TotalStudentsEnrolled,
    TotalTopics
}
public sealed record CourseSearchQueryAdminRequest(
    string? Title = null,
    int Take = 30,
    int Page = 1,
    CourseSortBy SortBy = CourseSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null
);
#endregion


#region Topic Search DTOs
public enum TopicSortBy
{
    CreatedAt,
    UpdatedAt,
    OrderIndex,
    ExamYear,
    TotalExercises
}
public sealed record TopicSearchQueryRequest(
    Guid CourseId,
    string? Title = null,
    bool? IsPublished = null,
    string? TopicType = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    int Page = 1,
    TopicSortBy SortBy = TopicSortBy.CreatedAt,
    bool OrderByDescending = true);
#endregion

#region ProgressTopicSearch
public sealed record TopicProgressCreatedQueriesRequest(
    Guid TopicProgressId,
    int TotalAnswered,
    int TotalCorrect,
    int TotalAttempts,
    DateTime CreatedAt,
    Guid UserId,
    Guid TopicId,
    float? AccuracyRate,
    int TotalWrong,
    float TotalScore
);

public enum ExerciseSessionItemStatus
{
    Pending,
    Viewed,
    Completed,
    Skipped
}

public sealed record ExerciseSessionStartedQueriesRequest(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    int HskLevel,
    DateTime StartedAt,
    StatusTopicForDashboardClient Status
);

public sealed record ResultCompleteSessionRequest(Guid SessionId, Guid UserId);

public sealed record ExerciseSessionItemsSnapshotRequest(
    Guid SessionId,
    // fallback to topicId and userId if sessionId is not found in cache, to at least update the progress based on topicId and userId, although it won't have the details of each exercise session item
    string Slug,
    Guid UserId
);

public sealed record ExerciseSessionItemSnapshot(
    Guid SessionItemId,
    Guid ExerciseId,
    int SequenceNo,
    int OrderIndex,
    Guid? AttemptId,
    ExerciseSessionItemStatus Status,
    DateTime? ViewedAt,
    DateTime? AnsweredAt
);

public sealed record ExerciseSessionSnapshotInitializedQueriesRequest(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    int HskLevel,
    int TotalExercises,
    int CurrentSequenceNo,
    List<ExerciseSessionItemSnapshot> SessionItems,
    DateTime InitializedAt,
    DateTime UpdatedAt
);
#endregion