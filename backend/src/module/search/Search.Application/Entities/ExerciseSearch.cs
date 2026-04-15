using HanziAnhVu.Shared.Domain;

namespace Search.Application.Entities;

public class ExerciseSearch
{
    public Guid ExerciseId { get; set; }
    public Guid TopicId { get; set; }
    public string Question { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CorrectAnswer { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public SkillType SkillType { get; set; }
    public ExerciseDifficulty Difficulty { get; set; }
    public ExerciseContext Context { get; set; }
    public string? AudioUrl { get; set; }
    public string Slug { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExerciseOption> Options { get; set; } = null!;

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