using HanziAnhVu.Shared.Domain;
using Lesson.Domain.Entities.Events;

namespace Lesson.Domain.Entities.Exercise;

public enum ExerciseSessionStatus
{
    InProgress = 0,
    Completed = 1,
    Abandoned = 2
}

/// <summary>
/// Phiên làm bài (Practice Session Aggregate)
/// - Group multiple exercises OR start standalone
/// - Track session score, status, time spent
/// - Aggregate Root for exercise attempts
/// </summary>
public class UserTopicExerciseSessionAggregate : BaseAggregateRoot
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; } // Soft ref to users.id (cross-module)
    public Guid? TopicId { get; private set; } // Optional: FK to topic.id (same module)
    public ExerciseSessionStatus Status { get; private set; } = ExerciseSessionStatus.InProgress;
    public float? TotalScore { get; private set; } // Calculated from exercise attempts
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TimeSpentSeconds { get; private set; } = 0;
    
    // Child entities (read-only)
    private readonly List<Guid> _attemptIds = [];
    public IReadOnlyList<Guid> AttemptIds => _attemptIds.AsReadOnly();
    
    protected UserTopicExerciseSessionAggregate() { }
    
    /// <summary>
    /// Factory method: Create new session (standalone or with topic context)
    /// </summary>
    public static UserTopicExerciseSessionAggregate Create(Guid userId, Guid? topicId = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId không được để trống");
        
        var session = new UserTopicExerciseSessionAggregate
        {
            SessionId = Guid.CreateVersion7(),
            UserId = userId,
            TopicId = topicId,
            Status = ExerciseSessionStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        
        session.AddDomainEvent(new UserTopicExerciseSessionStartedEvent(
            session.SessionId,
            userId,
            topicId,
            session.StartedAt,
            session.StartedAt
        ));
        return session;
    }
    
    /// <summary>
    /// Record an exercise attempt within this session
    /// </summary>
    public void RecordAttempt(Guid attemptId)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot add attempt to {Status} session");
        
        if (_attemptIds.Contains(attemptId))
            throw new InvalidOperationException($"Attempt {attemptId} already in session");
        
        _attemptIds.Add(attemptId);
    }
    
    /// <summary>
    /// Complete session with final score
    /// </summary>
    public void Complete(float sessionScore)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete {Status} session");
        
        if (_attemptIds.Count == 0)
            throw new InvalidOperationException("Cannot complete session with no attempts");
        
        Status = ExerciseSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        TotalScore = sessionScore;
        
        AddDomainEvent(new UserTopicExerciseSessionCompletedEvent(
            SessionId,
            UserId,
            TopicId,
            sessionScore,
            _attemptIds.Count,
            TimeSpentSeconds,
            CompletedAt.Value,
            CompletedAt.Value
        ));
    }
    
    /// <summary>
    /// Abandon session (user leaves without completing)
    /// </summary>
    public void Abandon()
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot abandon {Status} session");
        
        Status = ExerciseSessionStatus.Abandoned;
        CompletedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserTopicExerciseSessionAbandonedEvent(
            SessionId,
            UserId,
            TopicId,
            CompletedAt.Value,
            CompletedAt.Value
        ));
    }
    
    /// <summary>
    /// Update time spent (called periodically or on completion)
    /// </summary>
    public void UpdateTimeSpent(int totalSeconds)
    {
        if (totalSeconds < 0)
            throw new ArgumentException("Time spent cannot be negative");
        
        TimeSpentSeconds = totalSeconds;
    }
}

