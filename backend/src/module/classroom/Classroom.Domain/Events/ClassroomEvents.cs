using Classroom.Domain.Entities.Assignment;
using MediatR;

namespace Classroom.Domain.Events;

// --------------- CLASSROOM EVENTS --------------
public sealed record ClassroomCreatedEvent(
    Guid ClassroomId,
    string Title,
    string Description,
    Guid TeacherId,
    int HskLevel,
    DateTime? StartDate,
    DateTime? EndDate,
    string ScheduleInfo,
    float Price,
    Currency PriceCurrency,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Slug, 
    string ClassroomStatus
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Multiple students enrolled in classroom (BULK OPERATION)
/// Contains: List of student IDs enrolled in single operation
/// Denormalized: Classroom context for Search module
/// </summary>
public sealed record ClassroomStudentsEnrolledBulkEvent(
    Guid ClassroomId,
    List<Guid> StudentIds,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Individual student added to classroom
/// (Single operation, not bulk)
/// </summary>
public sealed record ClassroomStudentIndividualAddedEvent(
    Guid ClassroomId,
    Guid StudentId,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Individual student removed from classroom
/// (Single operation, not bulk)
/// </summary>
public sealed record ClassroomStudentIndividualRemovedEvent(
    Guid ClassroomId,
    Guid StudentId,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomStudentsRemovedBulkEvent(
    Guid ClassroomId,
    List<Guid> StudentIds,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomPublishedEvent(
    Guid ClassroomId,
    DateTime PublishedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomUnpublishedEvent(
    Guid ClassroomId,
    DateTime UnpublishedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomStatusChangedEvent(
    Guid ClassroomId,
    string NewStatus,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// Granular Classroom events
public sealed record ClassroomTitleUpdatedEvent(
    Guid ClassroomId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomDescriptionUpdatedEvent(
    Guid ClassroomId,
    string NewDescription,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomHskLevelUpdatedEvent(
    Guid ClassroomId,
    int NewHskLevel,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomStartDateUpdatedEvent(
    Guid ClassroomId,
    DateTime NewStartDate,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomEndDateUpdatedEvent(
    Guid ClassroomId,
    DateTime NewEndDate,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomScheduleInfoUpdatedEvent(
    Guid ClassroomId,
    string NewScheduleInfo,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomPriceUpdatedEvent(
    Guid ClassroomId,
    float NewPrice,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record ClassroomPriceCurrencyUpdatedEvent(
    Guid ClassroomId,
    string NewCurrency,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// --------------- END CLASSROOM EVENTS --------------
// --------------- ASSIGNMENT EVENTS --------------

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
    bool IsTimedAssignment,         //  Có giới hạn thời gian làm bài
    DurationTimeMinutes? DurationMinutes,           //  Thời gian làm bài (phút)
    bool IsPublished,
    int ExerciseCount,
    int RecipientCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// --------------- END ASSIGNMENT EVENTS --------------

/// <summary>
/// Event: Exercise added to assignment
/// (May not be critical for Search, but useful for audit trail)
/// </summary>
public sealed record AssignmentExerciseAddedEvent(
    Guid AssignmentId,
    Guid ExerciseId,
    int OrderIndex,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Exercise removed from assignment
/// Triggers re-index of assignment with updated exerciseCount
/// </summary>
public sealed record AssignmentExerciseRemovedEvent(
    Guid AssignmentId,
    Guid ExerciseId,
    int UpdatedExerciseCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Recipient added to Individual assignment
/// (For tracking individual assignment recipients)
/// </summary>
public sealed record AssignmentRecipientAddedEvent(
    Guid AssignmentId,
    Guid StudentId,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Recipient removed from Individual assignment
/// </summary>
public sealed record AssignmentRecipientRemovedEvent(
    Guid AssignmentId,
    Guid StudentId,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Multiple recipients added to assignment (BULK OPERATION)
/// Contains: List of student IDs added in single operation
/// Denormalized: Assignment context for Search module
/// </summary>
public sealed record AssignmentRecipientsAddedBulkEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    List<Guid> StudentIds,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Multiple recipients removed from assignment (BULK OPERATION)
/// Contains: List of student IDs removed in single operation
/// Denormalized: Assignment context for Search module
/// </summary>
public sealed record AssignmentRecipientsRemovedBulkEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    List<Guid> StudentIds,
    int UpdatedRecipientCount,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Assignment published (RICH - Denormalized)
/// Published to Search module for indexing
/// Contains: Full assignment data needed for search
/// </summary>
public sealed record AssignmentPublishedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    bool IsPublished,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// Granular Assignment events
public sealed record AssignmentTitleUpdatedEvent(
    Guid AssignmentId,
    string NewTitle,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record AssignmentDescriptionUpdatedEvent(
    Guid AssignmentId,
    string NewDescription,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record AssignmentDueDateUpdatedEvent(
    Guid AssignmentId,
    DateTime NewDueDate,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record AssignmentSkillFocusUpdatedEvent(
    Guid AssignmentId,
    string NewSkillFocus,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record AssignmentTypeUpdatedEvent(
    Guid AssignmentId,
    string NewAssignmentType,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

public sealed record AssignmentDurationUpdatedEvent(
    Guid AssignmentId,
    string? NewDuration,
    bool IsTimedAssignment,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Assignment unpublished (removed from student view)
/// Triggers removal from Search index
/// </summary>
public sealed record AssignmentUnpublishedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    bool IsPublished,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// ============= ASSIGNMENT SUBMISSION EVENTS =============

public sealed record AssignmentSubmissionCreatedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    string Status,
    DateTime? SubmittedAt,
    float? TotalScore,
    string? TeacherFeedback,
    DateTime? GradedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Student started assignment submission (RICH)
/// Contains: Duration selected + deadline calculated
/// </summary>
public sealed record AssignmentSubmissionStartedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    string Status,
    DateTime StartedAt,
    DateTime DeadlineAt,
    int SelectedDurationMinutes,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Student submitted assignment (RICH)
/// Contains: Submission metrics for analytics
/// </summary>
public sealed record AssignmentSubmissionSubmittedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    int TotalAnswers,
    DateTime SubmittedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Submission finalized (auto-submit when time expires)
/// Contains: Reason + finalized timestamp
/// </summary>
public sealed record SubmissionFinalizedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    string Reason,              // "Time limit exceeded" hoặc other
    DateTime FinalizedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Assignment submission graded (RICH)
/// Contains: Final grade + feedback
/// Published to Search for analytics + student notification
/// </summary>
public sealed record AssignmentSubmissionGradedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    float TotalScore,
    string TeacherFeedback,
    int TotalCorrect,
    int TotalWrong,
    DateTime GradedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;


public sealed record SubmissionAnswerAddedEvent(
    Guid SubmissionId,
    Guid ExerciseId,
    Guid StudentId,
    string Answer,
    DateTime AddedAt,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Individual exercise answer graded (RICH)
/// (For tracking individual exercise scores within submission)
/// </summary>
public sealed record SubmissionAnswerGradedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    Guid AnswerId,
    bool IsCorrect,
    float Score,
    string Feedback,
    DateTime UpdatedAt
) : BaseDomainEvent, INotification;

// ============= CLASSROOM ENROLLMENT EVENTS =============

/// <summary>
/// Event: Enrollment created (RICH)
/// Student initiates enrollment, status = Pending
/// </summary>
public sealed record ClassroomEnrollmentCreatedEvent(
    Guid EnrollmentId,
    Guid ClassroomId,
    Guid StudentId,
    string Status,  // "Pending"
    DateTime EnrolledAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Enrollment confirmed (RICH)
/// Teacher approves enrollment after payment, status = Confirmed
/// Contains: Payment reference for audit trail
/// </summary>
public sealed record ClassroomEnrollmentConfirmedEvent(
    Guid EnrollmentId,
    Guid ClassroomId,
    Guid StudentId,
    Guid? PaymentId,
    string Status,  // "Confirmed"
    DateTime ConfirmedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Enrollment cancelled
/// Student or teacher cancels enrollment, status = Cancelled
/// </summary>
public sealed record ClassroomEnrollmentCancelledEvent(
    Guid EnrollmentId,
    Guid ClassroomId,
    Guid StudentId,
    string Status,  // "Cancelled"
    DateTime CancelledAt
) : BaseDomainEvent, INotification;

// ============= CLASSROOM STUDENT EVENTS =============

/// <summary>
/// Event: Student added to classroom (RICH)
/// Emitted when teacher confirms enrollment and student joins classroom
/// Contains: Who added the student (AddedBy) for audit trail
/// </summary>
public sealed record ClassroomStudentAddedEvent(
    Guid ClassroomStudentId,
    Guid ClassroomId,
    Guid StudentId,
    Guid AddedBy,
    string Status,  // "Active"
    DateTime JoinedAt
) : BaseDomainEvent, INotification;

/// <summary>
/// Event: Student removed from classroom
/// Status change from Active to Removed
/// </summary>
public sealed record ClassroomStudentRemovedEvent(
    Guid ClassroomStudentId,
    Guid ClassroomId,
    Guid StudentId,
    string Status,  // "Removed"
    DateTime RemovedAt
) : BaseDomainEvent, INotification;
