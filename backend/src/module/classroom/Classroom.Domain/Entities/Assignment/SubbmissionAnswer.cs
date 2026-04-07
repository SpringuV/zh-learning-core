using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities.Assignment;

public class SubmissionAnswer : Entity
{
    public Guid SubmissionId { get; private set; } // FK → AssignmentSubmissionAggregate
    public Guid ExerciseId { get; private set; } // Soft ref → exercise.id (cross-module)
    public string Answer { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    public float? Score { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static SubmissionAnswer Create(Guid submissionId, Guid exerciseId, string answer)
    {
        if (submissionId == Guid.Empty)
            throw new ArgumentException("SubmissionId không được để trống.", nameof(submissionId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId không được để trống.", nameof(exerciseId));
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Answer không được để trống.", nameof(answer));

        var submissionAnswer = new SubmissionAnswer
        {
            Id = Guid.CreateVersion7(), // ✅ Use Id from Entity base class
            SubmissionId = submissionId,
            ExerciseId = exerciseId,
            Answer = answer,
            IsCorrect = false,
            Feedback = string.Empty,
            Score = null,
            CreatedAt = DateTime.UtcNow
        };

        return submissionAnswer;
    }

    /// <summary>
    /// Grade this submission answer
    /// </summary>
    public void Grade(bool isCorrect, float score, string feedback)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 0 and 100.");

        IsCorrect = isCorrect;
        Score = score;
        Feedback = feedback ?? string.Empty;
    }
}