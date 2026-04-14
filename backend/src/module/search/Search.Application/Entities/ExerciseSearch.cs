using HanziAnhVu.Shared.Domain;

namespace Search.Application.Entities;

public class ExerciseSearch
{
    public Guid ExerciseId { get; private set; }
    public Guid TopicId { get; private set; }
    public string Question { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string CorrectAnswer { get; private set; } = null!;
    public ExerciseType ExerciseType { get; private set; }
    public SkillType SkillType { get; private set; }
    public ExerciseDifficulty Difficulty { get; private set; }
    public ExerciseContext Context { get; private set; }
    public string? AudioUrl { get; private set; }
    public string Slug { get; private set; } = null!;
    public int OrderIndex { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Explanation { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public List<ExerciseOption> Options { get; private set; } = null!;

    public ExerciseSearch() { }
    public ExerciseSearch(
        Guid exerciseId, 
        Guid topicId, 
        string question, 
        string description, 
        string correctAnswer, 
        string slug,
        ExerciseType exerciseType, 
        SkillType skillType, 
        ExerciseDifficulty difficulty, 
        ExerciseContext context, 
        int orderIndex,
        string? audioUrl, 
        string? imageUrl, 
        string? explanation, 
        bool isPublished, 
        DateTime createdAt, 
        DateTime updatedAt, 
        List<ExerciseOption> options)
    {
        ExerciseId = exerciseId;
        TopicId = topicId;
        Question = question;
        Description = description;
        CorrectAnswer = correctAnswer;
        Slug = slug;
        ExerciseType = exerciseType;
        SkillType = skillType;
        Difficulty = difficulty;
        OrderIndex = orderIndex;
        Context = context;
        AudioUrl = audioUrl;
        ImageUrl = imageUrl;
        Explanation = explanation;
        IsPublished = isPublished;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Options = options;
    }
}