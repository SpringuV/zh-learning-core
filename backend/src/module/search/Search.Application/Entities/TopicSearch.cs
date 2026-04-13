namespace Search.Application.Entities;

public enum TopicType
{
    Learning,
    Exam
}
public class TopicSearch
{
    public Guid TopicId { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public TopicType TopicType { get; set; }
    public long EstimatedTimeMinutes { get; set; }
    public int ExamYear { get; set; }
    public string ExamCode { get; set; } = null!;
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public long TotalExercises { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TopicSearch() { }
    public TopicSearch(
        Guid topicId,
        Guid courseId,
        string title,
        string description,
        string slug,
        TopicType topicType,
        int estimatedTimeMinutes,
        int examYear,
        string examCode,
        int orderIndex,
        bool isPublished,
        long totalExercises,
        DateTime createdAt,
        DateTime updatedAt)
    {
        TopicId = topicId;
        CourseId = courseId;
        Title = title;
        Description = description;
        Slug = slug;
        TopicType = topicType;
        EstimatedTimeMinutes = estimatedTimeMinutes;
        ExamYear = examYear;
        ExamCode = examCode;
        OrderIndex = orderIndex;
        TotalExercises = totalExercises;
        IsPublished = isPublished;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}