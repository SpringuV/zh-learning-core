namespace Lesson.Domain.Entities.Exercise;

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
    public Guid TopicId { get; private set; }
    public int HskLevel { get; private set; } // Denormalized from topic for easy access in session context
    public int CurrentSequenceNo { get; private set; } = 0; // For ordering attempts within session
    public ExerciseSessionStatus Status { get; private set; } = ExerciseSessionStatus.InProgress;
    public float? TotalScore { get; private set; } // Calculated from exercise attempts
    public int TotalExercises {get; private set; } // Count of exercises in the current snapshot
    public int TotalCorrect { get; private set; } = 0; // Count of correct exercises (for quick access without calculating from attempts)
    public int ScoreListening { get; private set; } = 0;
    public int ScoreReading { get; private set; } = 0;
    public int ScoreWriting { get; private set; } = 0;
    public int TotalWrong { get; private set; } = 0; // Count of wrong exercises (for quick access without calculating from attempts)
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TimeSpentSeconds { get; private set; } = 0;

    private readonly List<UserTopicExerciseSessionItem> _sessionItems = [];
    public IReadOnlyList<UserTopicExerciseSessionItem> SessionItems => _sessionItems.AsReadOnly();
    
    protected UserTopicExerciseSessionAggregate() { }
    [JsonConstructor] // Đánh dấu constructor này để System.Text.Json sử dụng khi deserialize từ JSON, đảm bảo các thuộc tính chỉ có getter vẫn được gán giá trị đúng cách khi đọc từ cache hoặc database.
    private UserTopicExerciseSessionAggregate(
        Guid sessionId,
        Guid userId,
        Guid topicId,
        int hskLevel,
        ExerciseSessionStatus status,
        DateTime startedAt,
        int currentSequenceNo)
    {
        SessionId = sessionId;
        UserId = userId;
        TopicId = topicId;
        HskLevel = hskLevel;
        Status = status;
        StartedAt = startedAt;
        CurrentSequenceNo = currentSequenceNo;
    }
    
    /// <summary>
    /// Factory method: Create new session (standalone or with topic context)
    /// </summary>
    public static UserTopicExerciseSessionAggregate Create(Guid userId, Guid topicId, int hskLevel)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId không được để trống");
        if (topicId == Guid.Empty) throw new ArgumentException("TopicId không được để trống");
        if (hskLevel < 1) throw new ArgumentOutOfRangeException(nameof(hskLevel), "HskLevel must be greater than 0");

        var session = new UserTopicExerciseSessionAggregate
        {
            SessionId = Guid.CreateVersion7(),
            UserId = userId,
            TopicId = topicId,
            Status = ExerciseSessionStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CurrentSequenceNo = 0,
            HskLevel = hskLevel
        };
        
        session.AddDomainEvent(new UserTopicExerciseSessionStartedEvent(
            session.SessionId,
            userId,
            topicId,
            StartedAt: session.StartedAt,
            Status: session.Status,
            HskLevel: session.HskLevel
        ));
        return session;
    }
    public void SetTotalCorrect(int totalCorrect)
    {
        TotalCorrect = totalCorrect;
    }
    public void SetScoreListening(int scoreListening)
    {
        ScoreListening = scoreListening;
    }
    public void SetScoreReading(int scoreReading)
    {
        ScoreReading = scoreReading;
    }
    public void SetScoreWriting(int scoreWriting)
    {
        ScoreWriting = scoreWriting;
    }
    public void SetTotalWrong(int totalWrong)
    {
        TotalWrong = totalWrong;
    }
    public void SetTotalExercises(int totalExercises)
    {
        TotalExercises = totalExercises;
    }

    public void SetTimeSpent(int totalSeconds)
    {
        if (totalSeconds < 0)
            throw new ArgumentException("Time spent cannot be negative", nameof(totalSeconds));
        
        TimeSpentSeconds = totalSeconds;
    }
    public void UpdateCurrentSequenceNo(int currentSequenceNo)
    {
        if (currentSequenceNo < 1)
            throw new ArgumentOutOfRangeException(nameof(currentSequenceNo), "CurrentSequenceNo must be greater than 0");
        
        CurrentSequenceNo = currentSequenceNo;
        AddDomainEvent(new UserTopicExerciseSessionSequenceUpdatedEvent(
            SessionId,
            UserId,
            TopicId,
            CurrentSequenceNo,
            UpdatedAt: DateTime.UtcNow
        ));
    }
    public void SetTotalScore(float totalScore)
    {
        if (totalScore < 0)
            throw new ArgumentException("Total score cannot be negative", nameof(totalScore));
        
        TotalScore = totalScore;
    }

    public void FinishSession()
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot finish {Status} session");
        Status = ExerciseSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserTopicExerciseSessionCompletedEvent(
            SessionId,
            UserId,
            TopicId,
            HskLevel: HskLevel,
            TotalExercises: TotalExercises,
            TotalScore: TotalScore ?? 0f,
            TotalCorrect: TotalCorrect,
            TotalWrong: TotalWrong,
            ScoreListening: ScoreListening,
            TimeSpentSeconds: TimeSpentSeconds,
            Status: Status,
            ScoreReading: ScoreReading,
            CompletedAt: CompletedAt.Value
        ));
    }

    public void AddBatchScoreEvent(IReadOnlyList<ExerciseAttemptBatchScoredItemDTO> attempts, int gradedCount, int pendingCount)
    {
        AddDomainEvent(new ExerciseAttemptBatchScoredEvent(
            SessionId,
            UserId,
            TopicId,
            Attempts: attempts
        ));
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

        // Emit a single snapshot-initialized event with the denormalized item list for read models.
        var itemSnapshots = _sessionItems
            .Select(item => new UserTopicExerciseSessionItemSnapshot(
                item.SessionItemId,
                item.ExerciseId,
                item.SequenceNo,
                item.OrderIndex,
                item.AttemptId,
                item.Status,
                item.ViewedAt,
                item.AnsweredAt))
            .ToList();

        var now = DateTime.UtcNow;
        AddDomainEvent(new UserTopicExerciseSessionSnapshotInitializedEvent(
            SessionId: SessionId,
            UserId: UserId,
            TopicId: TopicId,
            TotalExercises: TotalExercises,
            CurrentSequenceNo: CurrentSequenceNo,
            SessionItems: itemSnapshots,
            InitializedAt: now,
            UpdatedAt: now,
            HskLevel: HskLevel
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

    // Record an attempt for the current exercise in session. This will either create a new attempt or update the existing attempt for the exercise-session combination.
    public void RecordAttempt(Guid attemptId)
        => RecordAttempt(attemptId, CurrentSequenceNo);

    public void RecordAttempt(Guid attemptId, int sequenceNo)
    {
        if (Status != ExerciseSessionStatus.InProgress)
            throw new InvalidOperationException($"Cannot add attempt to {Status} session");
        
        if (attemptId == Guid.Empty)
            throw new ArgumentException("AttemptId không được để trống", nameof(attemptId));
       
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
            HskLevel: HskLevel,
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

