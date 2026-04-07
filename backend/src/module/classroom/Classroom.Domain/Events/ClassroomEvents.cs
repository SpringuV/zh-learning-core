using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Events;


/// <summary>
/// Event: Assignment created (RICH - Denormalized)
/// Contains: Full assignment metadata for Search module indexing
/// </summary>
public sealed record AssignmentCreatedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    Guid TeacherId,
    string Title,
    string Description,
    string AssignmentType,         // "AllClass" | "Individual"
    string SkillFocus,              // "Reading" | "Writing" | "Listening" | "Speaking"
    DateTime DueDate,
    bool IsPublished,
    int ExerciseCount,
    int RecipientCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise added to assignment
/// (May not be critical for Search, but useful for audit trail)
/// </summary>
public sealed record AssignmentExerciseAddedEvent(
    Guid AssignmentId,
    Guid ExerciseId,
    int OrderIndex,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Exercise removed from assignment
/// Triggers re-index of assignment with updated exerciseCount
/// </summary>
public sealed record AssignmentExerciseRemovedEvent(
    Guid AssignmentId,
    Guid ExerciseId,
    int UpdatedExerciseCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Recipient added to Individual assignment
/// (For tracking individual assignment recipients)
/// </summary>
public sealed record AssignmentRecipientAddedEvent(
    Guid AssignmentId,
    Guid StudentId,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Recipient removed from Individual assignment
/// </summary>
public sealed record AssignmentRecipientRemovedEvent(
    Guid AssignmentId,
    Guid StudentId,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Assignment published (RICH - Denormalized)
/// Published to Search module for indexing
/// Contains: Full assignment data needed for search
/// </summary>
public sealed record AssignmentPublishedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    Guid TeacherId,
    string Title,
    string Description,
    string AssignmentType,
    string SkillFocus,
    DateTime DueDate,
    int ExerciseCount,
    int RecipientCount,
    DateTime PublishedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Assignment unpublished (removed from student view)
/// Triggers removal from Search index
/// </summary>
public sealed record AssignmentUnpublishedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    DateTime UnpublishedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

// ============= ASSIGNMENT SUBMISSION EVENTS =============

/// <summary>
/// Event: Student started assignment submission (RICH)
/// Contains: Assignment + student context
/// </summary>
public sealed record AssignmentSubmissionStartedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    Guid ClassroomId,
    string AssignmentTitle,  // From assignment (denormalized)
    DateTime StartedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Student submitted assignment (RICH)
/// Contains: Submission metrics for analytics
/// </summary>
public sealed record AssignmentSubmissionSubmittedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    Guid ClassroomId,
    int TotalAnswers,
    DateTime SubmittedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Assignment submission graded (RICH)
/// Contains: Final grade + feedback
/// Published to Search for analytics + student notification
/// </summary>
public sealed record AssignmentSubmissionGradedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    Guid TeacherId,
    Guid ClassroomId,
    float TotalScore,
    string TeacherFeedback,
    int TotalCorrect,
    int TotalWrong,
    DateTime GradedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Individual exercise answer graded (RICH)
/// (For tracking individual exercise scores within submission)
/// </summary>
public sealed record SubmissionAnswerGradedEvent(
    Guid SubmissionId,
    Guid ExerciseId,
    Guid StudentId,
    bool IsCorrect,
    float Score,
    string Feedback,
    DateTime ScoredAt,
    DateTime UpdatedAt
) : BaseDomainEvent;
