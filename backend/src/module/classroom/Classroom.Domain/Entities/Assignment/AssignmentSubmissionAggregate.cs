using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities.Assignment;

public enum SubmissionStatus
{
    NotStarted,
    InProgress,
    Submitted,
    Graded
}

/// <summary>
/// AssignmentSubmissionAggregate - Contains submission details and answers for a specific student's assignment
/// Child entities: SubmissionAnswer[]
/// </summary>
public class AssignmentSubmissionAggregate : BaseAggregateRoot
{
    public Guid SubmissionId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Guid StudentId { get; private set; } // Soft ref → users.id (cross-module)
    public SubmissionStatus Status { get; private set; } = SubmissionStatus.NotStarted;
    
    //  Duration (từ Assignment, không phải student chọn)
    public int? SelectedDurationMinutes { get; private set; }      // Duration minutes từ Assignment (lấy khi Start)
    public DateTime? StartedAt { get; private set; }               // Khi student start bài
    public DateTime? DeadlineAt { get; private set; }              // Calculated: StartedAt + SelectedDurationMinutes
    
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? FinalizedAt { get; private set; }             //  Khi auto-finalize (hết giờ hoặc submit)
    public float? TotalScore { get; private set; }
    public string TeacherFeedback { get; private set; } = string.Empty;
    public DateTime? GradedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Child entities collection
    private readonly List<SubmissionAnswer> _answers = [];
    public IReadOnlyList<SubmissionAnswer> Answers => _answers.AsReadOnly();

    public static AssignmentSubmissionAggregate Create(Guid assignmentId, Guid studentId)
    {
        if (assignmentId == Guid.Empty)
            throw new ArgumentException("AssignmentId không được để trống.", nameof(assignmentId));
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId không được để trống.", nameof(studentId));

        var submission = new AssignmentSubmissionAggregate
        {
            SubmissionId = Guid.CreateVersion7(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            Status = SubmissionStatus.NotStarted,
            SubmittedAt = null,
            TotalScore = null,
            TeacherFeedback = string.Empty,
            GradedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        submission.AddDomainEvent(new AssignmentSubmissionCreatedEvent(
            submission.SubmissionId,
            submission.AssignmentId,
            submission.StudentId,
            submission.Status.ToString(),
            submission.SubmittedAt,
            submission.TotalScore,
            submission.TeacherFeedback,
            submission.GradedAt,
            submission.CreatedAt,
            submission.UpdatedAt
        ));
        return submission;
    }

    // add new answer to this submission
    public void AddAnswer(Guid exerciseId, string answer)
    {
        if (FinalizedAt.HasValue)
            throw new InvalidOperationException("Cannot add answer after finalization.");
        if (Status != SubmissionStatus.InProgress)
            throw new InvalidOperationException("Cannot add answer when submission is not in InProgress status.");

        var submissionAnswer = SubmissionAnswer.Create(SubmissionId, exerciseId, answer);
        _answers.Add(submissionAnswer);
        AddDomainEvent(new SubmissionAnswerAddedEvent(
            SubmissionId,
            exerciseId,
            StudentId,
            answer,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(int assignmentDurationMinutes)
    {
        if (Status != SubmissionStatus.NotStarted)
            throw new InvalidOperationException("Bài làm đã được bắt đầu.");
        
        if (assignmentDurationMinutes <= 0)
            throw new ArgumentException("Thời lượng bài làm phải lớn hơn 0.", nameof(assignmentDurationMinutes));

        Status = SubmissionStatus.InProgress;
        SelectedDurationMinutes = assignmentDurationMinutes;  // ← From Assignment, not student choice
        StartedAt = DateTime.UtcNow;
        DeadlineAt = StartedAt.Value.AddMinutes(assignmentDurationMinutes);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new AssignmentSubmissionStartedEvent(
            SubmissionId,
            AssignmentId,
            StudentId,
            Status.ToString(),
            StartedAt.Value,
            DeadlineAt.Value,
            SelectedDurationMinutes.Value,  // ← Already assigned above, guaranteed non-null
            UpdatedAt
        ));
    }

    public void Submit()
    {
        if (Status != SubmissionStatus.InProgress)
            throw new InvalidOperationException("Only in-progress submissions can be submitted.");
        if (_answers.Count == 0)
            throw new InvalidOperationException("Cannot submit without any answers.");

        Status = SubmissionStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentSubmissionSubmittedEvent(
            SubmissionId,
            AssignmentId,
            StudentId,
            _answers.Count,
            SubmittedAt.Value,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Finalize submission when time expires or manually
    /// After finalize, cannot add/change answers
    /// </summary>
    public void Finalize(string reason = "Thời gian đã kết thúc")
    {
        if (FinalizedAt.HasValue)
            throw new InvalidOperationException("Bài đã được hoàn thành.");
        
        // Auto-submit nếu chưa submit
        if (Status == SubmissionStatus.InProgress)
        {
            if (_answers.Count == 0)
                throw new InvalidOperationException("Không thể hoàn thành bài mà không có câu trả lời.");
            
            Status = SubmissionStatus.Submitted;
            SubmittedAt = DateTime.UtcNow;
        }
        
        FinalizedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubmissionFinalizedEvent(
            SubmissionId,
            AssignmentId,
            StudentId,
            reason,
            FinalizedAt.Value,
            UpdatedAt
        ));
    }

    public void Grade(float totalScore, string teacherFeedback)
    {
        if (totalScore < 0 || totalScore > 100)
            throw new ArgumentOutOfRangeException(nameof(totalScore), "Score must be between 0 and 100.");
        if (Status != SubmissionStatus.Submitted)
            throw new InvalidOperationException("Only submitted submissions can be graded.");

        TotalScore = totalScore;
        TeacherFeedback = teacherFeedback ?? string.Empty;
        Status = SubmissionStatus.Graded;
        GradedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // Calculate totals from answers
        var totalCorrect = _answers.Count(a => a.IsCorrect);
        var totalWrong = _answers.Count(a => !a.IsCorrect);
        
        AddDomainEvent(new AssignmentSubmissionGradedEvent(
            SubmissionId,
            AssignmentId,
            StudentId,
            TotalScore.Value,
            TeacherFeedback,
            totalCorrect,
            totalWrong,
            GradedAt.Value,
            UpdatedAt
        ));
    }

    public void GradeAnswer(Guid answerId, bool isCorrect, float score, string feedback)
    {
        if (Status != SubmissionStatus.Submitted && Status != SubmissionStatus.Graded)
            throw new InvalidOperationException("Chỉ có thể chấm câu trả lời khi bài đã nộp (Submitted).");
        
        if (score < 0)
            throw new ArgumentOutOfRangeException(nameof(score), "Score không được âm.");
        
        var answer = _answers.FirstOrDefault(a => a.Id == answerId) 
            ?? throw new ArgumentException($"Answer with Id {answerId} not found in this submission.", nameof(answerId));
        
        answer.Grade(isCorrect, score, feedback);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SubmissionAnswerGradedEvent(
            SubmissionId,
            AssignmentId,
            StudentId,
            answerId,
            isCorrect,
            score,
            feedback,
            DateTime.UtcNow
        ));
    }
}