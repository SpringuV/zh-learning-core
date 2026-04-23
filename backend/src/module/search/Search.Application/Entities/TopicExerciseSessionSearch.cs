namespace Search.Application.Entities;

public class TopicExerciseSessionSearch
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public int TotalExercises { get; set; }
    public int CurrentSequenceNo { get; set; }
    public IReadOnlyList<ExerciseSessionItemSnapshot> ExerciseItems { get; set; } = [];
    public DateTime InitializedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TopicExerciseSessionSearch() { }
    public TopicExerciseSessionSearch(Guid sessionId, Guid userId, Guid topicId, int totalExercises, int currentSequenceNo, IReadOnlyList<ExerciseSessionItemSnapshot> exerciseItems, DateTime initializedAt, DateTime updatedAt)
    {
        SessionId = sessionId;
        UserId = userId;
        TopicId = topicId;
        TotalExercises = totalExercises;
        CurrentSequenceNo = currentSequenceNo;
        ExerciseItems = exerciseItems;
        InitializedAt = initializedAt;
        UpdatedAt = updatedAt;
    }
}