namespace Search.Application.Entities;

public class TopicProgressSearch
{
    public Guid TopicProgressId { get; set; }
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalAnswered { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalWrong { get; set; }
    public int TotalAttempts { get; set; }
    public float TotalScore { get; set; }
    public float? AccuracyRate { get; set; }
    
    public TopicProgressSearch() { }
    public TopicProgressSearch(Guid topicProgressId, Guid userId, Guid topicId, int totalAnswered, int totalCorrect, int totalWrong, int totalAttempts, float totalScore, float? accuracyRate, DateTime createdAt, DateTime updatedAt)
    {
        TopicProgressId = topicProgressId;
        UserId = userId;
        TopicId = topicId;
        TotalAnswered = totalAnswered;
        TotalCorrect = totalCorrect;
        TotalWrong = totalWrong;
        TotalAttempts = totalAttempts;
        TotalScore = totalScore;
        AccuracyRate = accuracyRate;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}