namespace Lesson.Domain.Entities.Exercise;


public sealed record UserTopicExerciseSessionItemSnapshot(
    Guid SessionItemId,
    Guid ExerciseId,
    int SequenceNo,
    int OrderIndex,
    Guid? AttemptId,
    UserTopicExerciseSessionItemStatus Status,
    DateTime? ViewedAt,
    DateTime? AnsweredAt
);

/// <summary>
/// Snapshot item for one exercise inside a topic learning session.
/// Stores ordering and lightweight progress state only.
/// </summary>

// Note: This is a child entity of UserTopicExerciseSessionAggregate, 
// but we model it as a separate table for easier querying and 
// updating of individual items without loading the entire session aggregate.
public class UserTopicExerciseSessionItem
{
    public Guid SessionItemId { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int SequenceNo { get; private set; }
    public int OrderIndex { get; private set; }
    public Guid? AttemptId { get; private set; }
    public UserTopicExerciseSessionItemStatus Status { get; private set; } = UserTopicExerciseSessionItemStatus.Pending;
    public DateTime? ViewedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected UserTopicExerciseSessionItem() { }
    [JsonConstructor] // Đánh dấu constructor này để System.Text.Json sử dụng khi deserialize từ JSON, đảm bảo các thuộc tính chỉ có getter vẫn được gán giá trị đúng cách khi đọc từ cache hoặc database.
    private UserTopicExerciseSessionItem(
        Guid sessionItemId,
        Guid sessionId,
        Guid exerciseId,
        int sequenceNo,
        int orderIndex,
        Guid? attemptId,
        UserTopicExerciseSessionItemStatus status,
        DateTime? viewedAt,
        DateTime? answeredAt,
        DateTime createdAt,
        DateTime updatedAt)
    {
        SessionItemId = sessionItemId;
        SessionId = sessionId;
        ExerciseId = exerciseId;
        SequenceNo = sequenceNo;
        OrderIndex = orderIndex;
        AttemptId = attemptId;
        Status = status;
        ViewedAt = viewedAt;
        AnsweredAt = answeredAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static UserTopicExerciseSessionItem Create(
        Guid sessionId,
        Guid exerciseId,
        int sequenceNo,
        int orderIndex)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("SessionId không được để trống", nameof(sessionId));
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId không được để trống", nameof(exerciseId));
        if (sequenceNo < 1) throw new ArgumentOutOfRangeException(nameof(sequenceNo), "SequenceNo must be greater than 0");
        if (orderIndex < 1) throw new ArgumentOutOfRangeException(nameof(orderIndex), "OrderIndex must be greater than 0");

        return new UserTopicExerciseSessionItem
        {
            SessionItemId = Guid.CreateVersion7(),
            SessionId = sessionId,
            ExerciseId = exerciseId,
            SequenceNo = sequenceNo,
            OrderIndex = orderIndex,
            Status = UserTopicExerciseSessionItemStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void MarkViewed()
    {
        if (Status == UserTopicExerciseSessionItemStatus.Completed)
        {
            return;
        }
        DateTime now = DateTime.UtcNow;

        ViewedAt ??= now;
        if (Status == UserTopicExerciseSessionItemStatus.Pending)
        {
            Status = UserTopicExerciseSessionItemStatus.Viewed;
        }

        UpdatedAt = now;
    }

    public void MarkCompleted(Guid attemptId)
    {
        if (attemptId == Guid.Empty) throw new ArgumentException("AttemptId không được để trống", nameof(attemptId));

        if (AttemptId.HasValue && AttemptId.Value != attemptId)
        {
            throw new InvalidOperationException("Session item đã được gắn với một attempt khác.");
        }
        DateTime now = DateTime.UtcNow;

        AttemptId = attemptId;
        ViewedAt ??= now;
        AnsweredAt = now;
        Status = UserTopicExerciseSessionItemStatus.Completed;
        UpdatedAt = now;
    }
}