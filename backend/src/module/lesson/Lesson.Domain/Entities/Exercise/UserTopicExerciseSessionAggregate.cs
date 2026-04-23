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
    public int CurrentSequenceNo { get; private set; } = 0; // For ordering attempts within session
    public ExerciseSessionStatus Status { get; private set; } = ExerciseSessionStatus.InProgress;
    public float? TotalScore { get; private set; } // Calculated from exercise attempts
    public int TotalExercises {get; private set; } // Count of exercises in the current snapshot
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TimeSpentSeconds { get; private set; } = 0;

    private readonly List<UserTopicExerciseSessionItem> _sessionItems = [];
    public IReadOnlyList<UserTopicExerciseSessionItem> SessionItems => _sessionItems.AsReadOnly();
    
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
            StartedAt = DateTime.UtcNow,
            CurrentSequenceNo = 0
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
    /// Set the session snapshot items in their canonical order.
    /// </summary>
    public void SetSessionItems(IEnumerable<UserTopicExerciseSessionItem> sessionItems)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot set items for {Status} session");

        if (sessionItems is null)
            throw new ArgumentNullException(nameof(sessionItems));

        var orderedItems = sessionItems
            .OrderBy(item => item.SequenceNo)
            .ThenBy(item => item.OrderIndex)
            .ToList();

        _sessionItems.Clear();
        _sessionItems.AddRange(orderedItems);

        TotalExercises = _sessionItems.Count;
        // Reset to first item when setting new snapshot (e.g. when starting a new session or updating snapshot mid-session)
        CurrentSequenceNo = TotalExercises > 0 ? _sessionItems[0].SequenceNo : 0;

        var now = DateTime.UtcNow;
        AddDomainEvent(new UserTopicExerciseSessionSnapshotInitializedEvent(
            SessionId,
            UserId,
            TopicId,
            TotalExercises,
            CurrentSequenceNo,
            now,
            now
        ));
    }

    public UserTopicExerciseSessionItem GetSessionItemBySequenceNo(int sequenceNo)
    {
        if (sequenceNo < 1)
            throw new ArgumentOutOfRangeException(nameof(sequenceNo), "SequenceNo must be greater than 0");

        var item = _sessionItems.FirstOrDefault(x => x.SequenceNo == sequenceNo)
            ?? throw new InvalidOperationException($"Session item with sequence no {sequenceNo} was not found.");

        return item;
    }

    public UserTopicExerciseSessionItem GetCurrentSessionItem()
    {
        if (CurrentSequenceNo < 1)
            throw new InvalidOperationException("CurrentSequenceNo is not set.");

        return GetSessionItemBySequenceNo(CurrentSequenceNo);
    }

    // move to next sequence item (called after recording attempt for current item)
    public void MoveToSequenceNo(int sequenceNo)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot move in {Status} session");

        if (sequenceNo < 1)
            throw new ArgumentOutOfRangeException(nameof(sequenceNo), "SequenceNo must be greater than 0");

        if (_sessionItems.Count > 0 && _sessionItems.All(x => x.SequenceNo != sequenceNo))
            throw new InvalidOperationException($"SequenceNo {sequenceNo} is outside the current session snapshot.");

        CurrentSequenceNo = sequenceNo;
        var sessionItem = _sessionItems.FirstOrDefault(x => x.SequenceNo == sequenceNo);
        if (sessionItem is not null && sessionItem.Status == UserTopicExerciseSessionItemStatus.Pending)
        {
            sessionItem.MarkViewed();

            var now = DateTime.UtcNow;
            AddDomainEvent(new UserTopicExerciseSessionItemViewedEvent(
                SessionId,
                UserId,
                TopicId,
                sessionItem.SessionItemId,
                sessionItem.ExerciseId,
                sessionItem.SequenceNo,
                sessionItem.OrderIndex,
                sessionItem.ViewedAt ?? now,
                now
            ));
        }
    }

    public void RecordAttempt(Guid attemptId)
        => RecordAttempt(attemptId, CurrentSequenceNo);

    public void RecordAttempt(Guid attemptId, int sequenceNo)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot add attempt to {Status} session");
        
        if (attemptId == Guid.Empty)
            throw new ArgumentException("AttemptId không được để trống", nameof(attemptId));

        if (_attemptIds.Contains(attemptId))
            throw new InvalidOperationException($"Attempt {attemptId} already in session");
        
        var sessionItem = GetSessionItemBySequenceNo(sequenceNo);
        sessionItem.MarkCompleted(attemptId);

        var now = DateTime.UtcNow;
        AddDomainEvent(new UserTopicExerciseSessionItemCompletedEvent(
            SessionId,
            UserId,
            TopicId,
            sessionItem.SessionItemId,
            attemptId,
            sessionItem.ExerciseId,
            sessionItem.SequenceNo,
            sessionItem.OrderIndex,
            sessionItem.AnsweredAt ?? now,
            now
        ));

        _attemptIds.Add(attemptId);
    }

    public void SkipCurrentSequence()
        => SkipSequenceNo(CurrentSequenceNo);

    public void SkipSequenceNo(int sequenceNo)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot skip in {Status} session");

        if (sequenceNo < 1)
            throw new ArgumentOutOfRangeException(nameof(sequenceNo), "SequenceNo must be greater than 0");

        var sessionItem = GetSessionItemBySequenceNo(sequenceNo);
        if (sessionItem.Status is UserTopicExerciseSessionItemStatus.Completed or UserTopicExerciseSessionItemStatus.Skipped)
        {
            return;
        }

        sessionItem.MarkSkipped();

        var now = DateTime.UtcNow;
        AddDomainEvent(new UserTopicExerciseSessionItemSkippedEvent(
            SessionId,
            UserId,
            TopicId,
            sessionItem.SessionItemId,
            sessionItem.ExerciseId,
            sessionItem.SequenceNo,
            sessionItem.OrderIndex,
            now,
            now
        ));
    }
    
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

