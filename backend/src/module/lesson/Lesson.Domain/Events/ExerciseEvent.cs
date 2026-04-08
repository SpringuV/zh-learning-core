using HanziAnhVu.Shared.Domain;
using Lesson.Domain.Entities.Exercise;

/// <summary>
/// Domain Events - Rich events with denormalized payload for Search module
/// Pattern: Events contain full context → Search handlers DON'T query DB
/// </summary>
namespace Lesson.Domain.Entities.Events;

// --------------- Topic events --------------
public record TopicCreatedEvent(
    Guid TopicId,
    Guid CourseId,
    bool IsPublished,
    string Title,
    string Description,
    string Slug,
    TopicType TopicType,
    int EstimatedTimeMinutes,
    int? ExamYear,
    string ExamCode,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt
): BaseDomainEvent;

public record TopicUpdatedEvent(
    Guid TopicId,
    Guid CourseId,
    bool? IsPublished,
    string? Title,
    string? Description,
    string? Slug,
    TopicType? TopicType,
    int? EstimatedTimeMinutes,
    int? ExamYear,
    string? ExamCode,
    int? OrderIndex,
    DateTime UpdatedAt
): BaseDomainEvent;

public record TopicPublishedEvent(
    Guid TopicId,
    DateTime PublishedAt
): BaseDomainEvent;

public record TopicUnpublishedEvent(
    Guid TopicId,
    DateTime UnpublishedAt
): BaseDomainEvent;

public record ExerciseAddedToTopicEvent(
    Guid TopicId,
    Guid ExerciseId,
    DateTime AddedAt
): BaseDomainEvent;

public record ExerciseRemovedFromTopicEvent(
    Guid TopicId,
    Guid ExerciseId,
    DateTime RemovedAt
): BaseDomainEvent;

// --------------- course events --------------
public record CourseCreatedEvent(
    Guid CourseId,
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Slug
): BaseDomainEvent;

/// <summary>
/// Event: Course title updated (and slug regenerated)
/// </summary>
public sealed record CourseTitleUpdatedEvent(
    Guid CourseId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt
) : BaseDomainEvent;

public record CourseEnrollmentCountUpdatedEvent(
    Guid CourseId,
    long TotalStudentsEnrolled,
    DateTime UpdatedAt
): BaseDomainEvent;

public record CoursePublishedDomainEvent(
    Guid CourseId,
    DateTime PublishedAt
): BaseDomainEvent;

public record CourseUnpublishedDomainEvent(
    Guid CourseId,
    DateTime UnpublishedAt
): BaseDomainEvent;

public record TopicAddedToCourseDomainEvent(
    Guid CourseId,
    Guid TopicId,
    int OrderIndex,
    DateTime AddedAt
): BaseDomainEvent;
// ============= EXERCISE EVENTS =============

/// <summary>
/// Event: Exercise created
/// Published to Search module for caching exercise metadata
/// </summary>
public sealed record ExerciseCreatedEvent(
    Guid ExerciseId,
    Guid TopicId,
    string Description,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsPublished,
    string ExerciseType,
    string SkillType,
    string Question,
    string CorrectAnswer,
    string Difficulty,
    string Context,
    string AudioUrl,
    string ImageUrl,
    string Explanation,
    string Slug,
    List<ExerciseOption>? Options
) : BaseDomainEvent;

// bulk update
public sealed record ExerciseUpdatedEvent(
    Guid ExerciseId,
    Guid TopicId,
    string? Description,
    int? OrderIndex,
    DateTime UpdatedAt,
    string? Question,
    string? CorrectAnswer,
    string? Difficulty,
    string? Context,
    string? AudioUrl,
    string? ImageUrl,
    string? Explanation,
    string? Slug,
    List<ExerciseOption>? Options
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise published (accessible to students/classrooms)
/// Minimal: key + timestamp for partial update (only IsPublished flag changed)
/// </summary>
public sealed record ExercisePublishedEvent(
    Guid ExerciseId,
    DateTime PublishedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise unpublished (no longer accessible to students)
/// </summary>
public sealed record ExerciseUnpublishedEvent(
    Guid ExerciseId,
    DateTime UnpublishedAt
) : BaseDomainEvent;
// ============= USER EXERCISE SESSION EVENTS =============

/// <summary>
/// Event: User started exercise session (RICH)
/// Rich: User + Topic context for Search module
/// </summary>
public sealed record UserTopicExerciseSessionStartedEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    DateTime StartedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: User completed exercise session (RICH)
/// Rich: Full session metrics for analytics + Search indexing
/// </summary>
public sealed record UserTopicExerciseSessionCompletedEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    float SessionScore,
    int TotalAttempts,
    int TimeSpentSeconds,
    DateTime CompletedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: User abandoned exercise session
/// </summary>
public sealed record UserTopicExerciseSessionAbandonedEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    DateTime AbandonedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

// ============= EXERCISE ATTEMPT EVENTS =============

/// <summary>
/// Event: Exercise attempt created (RICH EVENT - Denormalized payload)
/// Contains: Session context (userId, topicId) + Exercise metadata (title, skillType, etc.)
/// Usage: Search module indexes directly from event (NO DB queries needed)
/// </summary>
public sealed record ExerciseAttemptCreatedEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,             // From session (denormalized)
    Guid? TopicId,           // From session (nullable)
    Guid ExerciseId,
    string Question,         // From exercise
    string ExerciseType,     // From exercise (denormalized)
    string SkillType,        // From exercise (denormalized)
    string Difficulty,       // From exercise (denormalized)
    string Answer,           // User's answer
    float InitialScore,      // 0 at creation, updated when scored
    bool IsCorrect,          // false at creation, updated when scored
    DateTime CreatedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise attempt scored/graded (RICH)
/// Rich: Contains full context (userId, skillType) for Search metrics
/// </summary>
public sealed record ExerciseAttemptScoredEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    string SkillType,        // From exercise (denormalized)
    float Score,
    bool IsCorrect,
    DateTime ScoredAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: AI feedback added to exercise attempt
/// (For writing exercises or detailed analysis)
/// </summary>
public sealed record ExerciseAttemptAiFeedbackAddedEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    string Feedback,
    DateTime FeedbackAddedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise attempt answer changed during session
/// User changed their answer before finalizing session
/// </summary>
public sealed record ExerciseAttemptAnswerChangedEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    string NewAnswer,
    DateTime ChangedAt
) : BaseDomainEvent;

// ============= USER PROGRESS EVENTS =============

/// <summary>
/// Event: User progress updated after session completion
/// Published to Outbox for cross-module notifications (e.g., level-up notifications)
/// </summary>
public record UserTopicProgressCreatedEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    int TotalAttempts,
    int TotalAnswered,
    int TotalCorrect,
    int TotalWrong,
    float TotalScore,
    float? AccuracyRate,
    DateTime CreatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: User progress updated after session completion
/// Published to Outbox for cross-module notifications (e.g., level-up notifications)
/// </summary>
public record UserTopicProgressUpdatedEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    int? TotalAttempts,
    int? TotalAnswered,
    int? TotalCorrect,
    int? TotalWrong,
    float? TotalScore,
    float? AccuracyRate,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: User progress reset
/// </summary>
public record UserTopicProgressResetEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    DateTime ResetAt
) : BaseDomainEvent;

