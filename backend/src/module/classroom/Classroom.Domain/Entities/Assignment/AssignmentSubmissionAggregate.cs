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
    public DateTime? SubmittedAt { get; private set; }
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

        return submission;
    }

    public void AddAnswer(Guid exerciseId, string answer)
    {
        if (Status != SubmissionStatus.InProgress)
            throw new InvalidOperationException("Cannot add answer when submission is not in InProgress status.");

        var submissionAnswer = SubmissionAnswer.Create(SubmissionId, exerciseId, answer);
        _answers.Add(submissionAnswer);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != SubmissionStatus.NotStarted)
            throw new InvalidOperationException("Submission has already been started.");

        Status = SubmissionStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
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
    }

    public void GradeAnswer(Guid answerId, bool isCorrect, float score, string feedback)
    {
        var answer = _answers.FirstOrDefault(a => a.Id == answerId);
        if (answer == null)
            throw new ArgumentException($"Answer with Id {answerId} not found in this submission.", nameof(answerId));

        answer.Grade(isCorrect, score, feedback);
        UpdatedAt = DateTime.UtcNow;
    }
}