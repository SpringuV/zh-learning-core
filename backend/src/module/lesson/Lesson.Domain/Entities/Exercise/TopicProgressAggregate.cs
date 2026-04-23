namespace Lesson.Domain.Entities.Exercise;
/// <summary>
/// Aggregate Root: User's cumulative progress for a specific topic
/// Denormalized stats table for fast dashboard queries
/// Updated via domain events from UserExerciseSessionAggregate completions
/// </summary>
public class TopicProgressAggregate : BaseAggregateRoot
{
    public Guid TopicProgressId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TopicId { get; private set; }
    // Stats
    public int TotalAttempts { get; private set; }         // Số phiên làm bài
    public int TotalAnswered { get; private set; }        // Tổng số câu đã trả lời
    public int TotalCorrect { get; private set; }         // Tổng số câu đúng
    public int TotalWrong { get; private set; }           // Tổng số câu sai
    public float TotalScore { get; private set; }         // Điểm tích lũy
    public float? AccuracyRate { get; private set; }      // Tỉ lệ đúng (%)
    public DateTime? LastPracticedAt { get; private set; } // Lần luyện gần nhất
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public TopicProgressAggregate() { }
    
    /// <summary>
    /// Factory: Tạo progress tracking mới cho user + topic
    /// </summary>
    public static TopicProgressAggregate Create(Guid userId, Guid topicId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không được để trống", nameof(userId));
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId không được để trống", nameof(topicId));
        
        var progress = new TopicProgressAggregate
        {
            TopicProgressId = Guid.CreateVersion7(),
            UserId = userId,
            TopicId = topicId,
            TotalAttempts = 0,
            TotalAnswered = 0,
            TotalCorrect = 0,
            TotalWrong = 0,
            TotalScore = 0,
            AccuracyRate = null,
            LastPracticedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Emit event for new progress record
        progress.AddDomainEvent(new UserTopicProgressCreatedEvent(
            progress.TopicProgressId,
            userId,
            topicId,
            0,
            0,
            0,
            0,
            0,
            null,
            progress.CreatedAt
        ));
        
        return progress;
    }
    
    /// <summary>
    /// Update: Cộng dồn stats từ một phiên luyện bài đã hoàn thành
    /// Gọi từ Application Handler khi UserExerciseSessionCompletedEvent được trigger
    /// </summary>
    public void UpdateFromSessionCompletion(int attemptedCount, int correctCount, float sessionScore)
    {
        if (attemptedCount < 0)
            throw new ArgumentException("Số lần thử không được nhỏ hơn 0", nameof(attemptedCount));
        if (correctCount < 0)
            throw new ArgumentException("Số câu đúng không được nhỏ hơn 0", nameof(correctCount));
        if (correctCount > attemptedCount)
            throw new ArgumentException("Số câu đúng không được vượt quá số lần thử", nameof(correctCount));
        if (sessionScore < 0)
            throw new ArgumentException("Điểm phiên không được nhỏ hơn 0", nameof(sessionScore));
        
        // Cộng dồn
        TotalAttempts++;
        TotalAnswered += attemptedCount;
        TotalCorrect += correctCount;
        TotalWrong += attemptedCount - correctCount;
        TotalScore += sessionScore;
        LastPracticedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // Recalculate accuracy rate
        if (TotalAnswered > 0)
        {
            AccuracyRate = TotalCorrect * 100f / TotalAnswered;
        }
        
        // Domain event để publish qua Outbox
        AddDomainEvent(new UserTopicProgressUpdatedEvent(
            TopicProgressId,
            UserId,
            TopicId,
            TotalAttempts,
            TotalAnswered,
            TotalCorrect,
            TotalWrong,
            TotalScore,
            AccuracyRate,
            DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Reset: Xóa tiến độ (dùng khi course expired hoặc admin reset)
    /// </summary>
    public void ResetProgress()
    {
        TotalAttempts = 0;
        TotalAnswered = 0;
        TotalCorrect = 0;
        TotalWrong = 0;
        TotalScore = 0;
        AccuracyRate = null;
        LastPracticedAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserTopicProgressResetEvent(
            TopicProgressId,
            UserId,
            TopicId,
            DateTime.UtcNow
        ));
    }
}
