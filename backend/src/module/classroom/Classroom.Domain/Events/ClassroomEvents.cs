using Classroom.Domain.Entities.Assignment;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

/// <summary>
/// Event: Individual student added to classroom
/// (Single operation, not bulk)
/// </summary>
public sealed record ClassroomStudentIndividualAddedEvent(
    Guid ClassroomId,
    Guid StudentId,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Individual student removed from classroom
/// (Single operation, not bulk)
/// </summary>
public sealed record ClassroomStudentIndividualRemovedEvent(
    Guid ClassroomId,
    Guid StudentId,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

public sealed record ClassroomStudentsRemovedBulkEvent(
    Guid ClassroomId,
    List<Guid> StudentIds,
    int UpdatedStudentCount,
    DateTime UpdatedAt
) : BaseDomainEvent;

public sealed record ClassroomPublishedEvent(
    Guid ClassroomId,
    DateTime PublishedAt
) : BaseDomainEvent;

public sealed record ClassroomUnpublishedEvent(
    Guid ClassroomId,
    DateTime UnpublishedAt
) : BaseDomainEvent;

public sealed record ClassroomStatusChangedEvent(
    Guid ClassroomId,
    string NewStatus,
    DateTime UpdatedAt
) : BaseDomainEvent;

public sealed record ClassroomUpdatedEvent(
    Guid ClassroomId,
    string? NewTitle,
    string? NewDescription,
    int? NewHskLevel,
    DateTime? NewStartDate,
    DateTime? NewEndDate,
    string? NewScheduleInfo,
    float? NewPrice,
    Currency? NewPriceCurrency,
    DateTime UpdatedAt
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

public sealed record AssignmentUpdatedEvent(
    Guid AssignmentId,
    string? NewTitle,
    string? NewDescription,
    DateTime? NewDueDate,
    string? NewAssignmentType,
    string? NewSkillFocus,
    bool? NewIsTimedAssignment,     // Timed assignment status
    DurationTimeMinutes? NewDurationMinutes,        // Duration in minutes
    DateTime UpdatedAt
) : BaseDomainEvent;

/// <summary>
/// Event: Assignment unpublished (removed from student view)
/// Triggers removal from Search index
/// </summary>
public sealed record AssignmentUnpublishedEvent(
    Guid AssignmentId,
    Guid ClassroomId,
    bool IsPublished,
    DateTime UpdatedAt
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
    float TotalScore,
    string TeacherFeedback,
    int TotalCorrect,
    int TotalWrong,
    DateTime GradedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;


public sealed record SubmissionAnswerAddedEvent(
    Guid SubmissionId,
    Guid ExerciseId,
    Guid StudentId,
    string Answer,
    DateTime AddedAt,
    DateTime UpdatedAt
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;

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
) : BaseDomainEvent;
