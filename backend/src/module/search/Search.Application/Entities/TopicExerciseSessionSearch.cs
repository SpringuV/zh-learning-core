namespace Search.Application.Entities;


public class TopicExerciseSessionSearch
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public int TotalExercises { get; set; }
    public int HskLevel { get; set; }
    public float TotalScore { get; set; }
    public int ScoreListening { get; set; }
    public int ScoreReading { get; set; }
    public int ScoreWriting { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalWrong { get; set; }
    public int CurrentSequenceNo { get; set; }
    public int TimeSpentSeconds { get; set; }
    public List<ExerciseSessionItemSnapshot> ExerciseItems { get; set; } = [];
    public DateTime InitializedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public StatusTopicForDashboardClient Status { get; set; }

    public TopicExerciseSessionSearch() { }
    public TopicExerciseSessionSearch(
        Guid sessionId, 
        Guid userId, 
        Guid topicId, 
        int totalExercises, 
        int hskLevel, 
        int currentSequenceNo, 
        List<ExerciseSessionItemSnapshot> exerciseItems, 
        DateTime initializedAt, 
        DateTime updatedAt, 
        StatusTopicForDashboardClient status,
        int timeSpentSeconds = 0,
        float totalScore = 0f,
        int scoreListening = 0,
        int scoreReading = 0,
        int scoreWriting = 0,
        int totalCorrect = 0,
        int totalWrong = 0)
    {
        SessionId = sessionId;
        UserId = userId;
        TopicId = topicId;
        TotalExercises = totalExercises;
        HskLevel = hskLevel;
        TotalScore = totalScore;
        ScoreListening = scoreListening;
        ScoreReading = scoreReading;
        ScoreWriting = scoreWriting;
        TotalCorrect = totalCorrect;
        TotalWrong = totalWrong;
        TimeSpentSeconds = timeSpentSeconds;
        CurrentSequenceNo = currentSequenceNo;
        ExerciseItems = exerciseItems;
        Status = status;
        InitializedAt = initializedAt; 
        UpdatedAt = updatedAt;
    }
    public TopicExerciseSessionSearch(
        Guid sessionId, 
        Guid userId, 
        Guid topicId, 
        int totalExercises, 
        int hskLevel, 
        int currentSequenceNo, 
        List<ExerciseSessionItemSnapshot> exerciseItems, 
        DateTime initializedAt, 
        DateTime updatedAt,
        float totalScore = 0f,
        int timeSpentSeconds = 0,
        int scoreListening = 0,
        int scoreReading = 0,
        int scoreWriting = 0,
        int totalCorrect = 0,
        int totalWrong = 0)
    {
        SessionId = sessionId;
        UserId = userId;
        TopicId = topicId;
        TotalExercises = totalExercises;
        HskLevel = hskLevel;
        TimeSpentSeconds = timeSpentSeconds;
        TotalScore = totalScore;
        ScoreListening = scoreListening;
        ScoreReading = scoreReading;
        ScoreWriting = scoreWriting;
        TotalCorrect = totalCorrect;
        TotalWrong = totalWrong;
        CurrentSequenceNo = currentSequenceNo;
        ExerciseItems = exerciseItems;
        InitializedAt = initializedAt;
        UpdatedAt = updatedAt;
    }
}