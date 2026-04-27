using HanziAnhVu.Shared.Domain;

namespace Search.Application.Entities;

public class TopicExerciseAttemptSearch
{
    public Guid AttemptId { get; set; }
    public Guid SessionId { get; set; }
    public Guid ExerciseId { get; set; }
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public string Question { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Explanation { get; set; } = null!;
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string Answer { get; set; } = null!;
    public ExerciseDifficulty Difficulty { get; set; } // denormalize Difficulty here for easier query, avoid join with ExerciseSearch which is in different index. This is needed for dashboard to quickly show difficulty for each attempt without needing to call ExerciseSearch API for each attempt.
    public ExerciseType ExerciseType { get; set; } // denormalize ExerciseType here for easier query, avoid join with ExerciseSearch which is in different index
    public SkillType SkillType { get; set; } // denormalize SkillType here for easier query, avoid join with ExerciseSearch which is in different index
    public string CorrectAnswer { get; set; } = null!; // denormalize CorrectAnswer here for easier query, avoid join with ExerciseSearch which is in different index. This is needed for dashboard to quickly show correct/wrong answer without needing to call ExerciseSearch API for each attempt.
    public IReadOnlyList<ExerciseOption> Options { get; set; } = null!; // denormalize Options here for easier query, avoid join with ExerciseSearch which is in different index. This is needed for dashboard to quickly show options for each attempt without needing to call ExerciseSearch API for each attempt.
    public bool IsCorrect { get; set; }
    public float Score { get; set; }
    public DateTime AttemptedAt { get; set; }

    public TopicExerciseAttemptSearch() { }
    public TopicExerciseAttemptSearch(
        Guid attemptId, 
        Guid sessionId, 
        Guid exerciseId, 
        Guid userId, 
        Guid topicId, 
        string answer, 
        string question,
        string description,
        string explanation,
        string? audioUrl,
        string? imageUrl,
        ExerciseType exerciseType, 
        SkillType skillType,
        ExerciseDifficulty difficulty,
        string correctAnswer,
        IReadOnlyList<ExerciseOption> options,
        bool isCorrect, 
        float score, 
        DateTime attemptedAt)
    {
        AttemptId = attemptId;
        SessionId = sessionId;
        ExerciseId = exerciseId;
        UserId = userId;
        TopicId = topicId;
        Answer = answer;
        Question = question;
        Description = description;
        Explanation = explanation;
        AudioUrl = audioUrl;
        ImageUrl = imageUrl;
        ExerciseType = exerciseType;
        SkillType = skillType;
        Difficulty = difficulty;
        CorrectAnswer = correctAnswer;
        Options = options;
        IsCorrect = isCorrect;
        Score = score;
        AttemptedAt = attemptedAt;
    }
}