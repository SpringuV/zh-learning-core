namespace Lesson.Domain.Entities.Exercise;

/// <summary>
/// ExerciseAttemptAggregate (Aggregate Root)
/// - Individual exercise attempt within a session
/// - Child of UserExerciseSessionAggregate (can exist standalone for persistence)
/// - Stores: answer, score, correctness, AI feedback
/// </summary>
public class ExerciseAttemptAggregate : BaseAggregateRoot
{
    public Guid AttemptId { get; private set; }
    public Guid SessionId { get; private set; } // FK to user_exercise_session (same module)
    public Guid ExerciseId { get; private set; } // Soft ref to exercise.id (cross-module)
    public SkillType SkillType { get; private set; } // Denormalized from Exercise for easier querying
    public string Answer { get; private set; } = string.Empty;
    public float Score { get; private set; } = 0f;
    public bool IsCorrect { get; private set; } = false;
    public string AiFeedback { get; private set; } = string.Empty;
    public bool IsFinalized { get; private set; } = false;  // Locked after session ends
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow; 
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected ExerciseAttemptAggregate() { }
    
    /// <summary>
    /// Factory method: Create exercise attempt in a session
    /// Rich: requires full context from session + exercise for denormalized event
    /// </summary>
    public static ExerciseAttemptAggregate Create(
        Guid sessionId,
        Guid exerciseId,
        SkillType skillType,
        string answer)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("SessionId không được để trống");
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId không được để trống");
        if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Answer không được để trống");
        
        var attempt = new ExerciseAttemptAggregate
        {
            AttemptId = Guid.CreateVersion7(),
            SessionId = sessionId,
            ExerciseId = exerciseId,
            SkillType = skillType,
            Answer = answer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Emit rich event with full context (denormalized payload)
        // Search handler won't need to query DB for session/exercise context
        attempt.AddDomainEvent(new ExerciseAttemptCreatedEvent(
            AttemptId: attempt.AttemptId,
            SessionId: sessionId,
            ExerciseId: exerciseId,
            SkillType: skillType,
            Answer: answer,
            InitialScore: 0f,               // Initial score = 0
            IsCorrect: false,            // IsCorrect = false (updated when scored)
            CreatedAt: attempt.CreatedAt,
            UpdatedAt: attempt.UpdatedAt
        ));
        
        return attempt;
    }
    public void SetScore(float score)
    {
        Score = score;
    }
    public void UpdateAnswer(string newAnswer, Guid ExerciseId, Guid SessionId)
    {
        if (IsFinalized)
            throw new InvalidOperationException("Attempts đã được hoàn thành, không thể sửa đáp án");
        
        if (string.IsNullOrWhiteSpace(newAnswer))
            throw new ArgumentException("Đáp án không được để trống", nameof(newAnswer));
        
        Answer = newAnswer;
        Score = 0f;          // Reset score when changing answer
        IsCorrect = false;   // Reset correctness
        UpdatedAt = DateTime.UtcNow;
        
        // Emit event for tracking answer changes
        AddDomainEvent(new ExerciseAttemptAnswerChangedEvent(
            AttemptId,
            SessionId,
            ExerciseId,
            newAnswer,
            UpdatedAt
        ));
    }
    
    public void SetIsCorrect(bool isCorrect)
    {
        IsCorrect = isCorrect;
        UpdatedAt = DateTime.UtcNow;
    }
    /// <summary>
    /// Finalize attempt when session ends
    /// After finalize, answer and score are locked
    /// Optional: Grade immediately if auto-scorable, else score will come from background job
    /// </summary>
    public void Finalize(float? score = null, bool? isCorrect = null, bool emitScoreEvent = true)
    {
        if (IsFinalized)
            throw new InvalidOperationException("Attempt đã được hoàn thành rồi");
        
        IsFinalized = true;
        UpdatedAt = DateTime.UtcNow;
        
        // If score provided, record it immediately (for auto-scorable)
        if (score.HasValue && isCorrect.HasValue)
        {
            RecordScore(score.Value, isCorrect.Value, emitScoreEvent);
        }
        // Else: Score will come from async grading job
    }
    
    /// <summary>
    /// Record grading result (auto-scored)
    /// Rich: includes denormalized context for Search
    /// Can only be called during draft (before finalize) for immediate grading,
    /// or after finalize for async grading results
    /// </summary>
    public void RecordScore(float score, bool isCorrect, bool emitEvent = true)
    {
        if (score < 0 || score > 100)
            throw new ArgumentException("Điểm phải từ 0-100");
        
        Score = score;
        IsCorrect = isCorrect;
        UpdatedAt = DateTime.UtcNow;
        
        if (!emitEvent)
        {
            return;
        }
        
        // Emit rich event with denormalized data
        AddDomainEvent(new ExerciseAttemptScoredEvent(
            AttemptId,
            SessionId,
            ExerciseId,
            score,
            isCorrect,
            DateTime.UtcNow,
            UpdatedAt
        ));
    }
    
    /// <summary>
    /// Add AI feedback (for writing exercises or detailed analysis)
    /// Rich: includes denormalized context for Search
    /// Can be added before or after finalization
    /// </summary>
    public void SetAiFeedback(string feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback))
            throw new ArgumentException("Feedback không được để trống");
        
        AiFeedback = feedback;
        UpdatedAt = DateTime.UtcNow;
        
        // Emit rich event with denormalized data
        AddDomainEvent(new ExerciseAttemptAiFeedbackAddedEvent(
            AttemptId,
            SessionId,
            ExerciseId,
            feedback,
            DateTime.UtcNow,
            UpdatedAt
        ));
    }
}

/*
phase1: Session Start 🎯
    User → Create Session
    ├─ SessionId, UserId, TopicId
    └─ Event: SessionStartedEvent
        └─ Search indexes: session started
phase2: Attempt & Answer ✍️
    User answers Q1:
    ├─ Create ExerciseAttempt(sessionId, userId, exerciseId, answer="A")
    ├─ Event: AttemptCreatedEvent
    └─ Search indexes: new attempt with answer="A"

    User changes mind → Update answer:
    ├─ Attempt.UpdateAnswer("answer=B")
    ├─ Event: AnswerChangedEvent  
    └─ Search updates: answer="B"

    Can edit? ✅ YES (IsFinalized=false)
Phase 3: Auto-Grade (Immediate) ⚡
    MC/True-False/Matching:
    ├─ Instant: Compare answer vs CorrectAnswer
    ├─ Attempt.RecordScore(score=100, isCorrect=true)
    ├─ Event: ScoredEvent
    └─ Search updates: score=100, isCorrect=true

    User sees score immediately! 📊
Phase 4: AI-Grade (Async) 🤖
    Writing/Speaking exercises:
    ├─ Create Attempt (score=0, pending)
    ├─ Queue job: "Grade ExerciseAttemptId"
    └─ Background job (2-5s later):
        ├─ AI grades → score=85
        ├─ Attempt.RecordScore(85, true)
        ├─ Event: ScoredEvent
        └─ Search updates: score=85

    User sees score later! ⏳
Phase 5: Session Finish 🏁
    User clicks "Finish Session":
    ├─ Loop all attempts:
    │  ├─ If not scored: RecordScore(finalScore)
    │  └─ Attempt.Finalize(...)  ← IsFinalized=true
    │
    ├─ Calculate totalScore = Average of all attempts
    ├─ Session.Complete(totalScore=85)
    ├─ Event: SessionCompletedEvent
    └─ Search indexes: session complete + final score

    Can edit now? ❌ NO (IsFinalized=true)
Phase 6: Results & Dashboard 📈
    After completion:
    ├─ TopicProgressAggregate.UpdateProgress()
    ├─ Event: ProgressUpdatedEvent
    │  ├─ TotalAttempts: 5
    │  ├─ TotalCorrect: 4
    │  ├─ AccuracyRate: 80%
    │  └─ TotalScore: 85
    │
    ├─ Search indexes: user progress updated
    └─ Dashboard shows:
        ├─ Score: 85/100 ✅
        ├─ Accuracy: 80%
        ├─ Time: 15 min
        └─ Breakdown by skill
*/