namespace Search.Application.Entities;

public class CourseSearch
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public int HskLevel { get; set; }
    public string Slug { get; set; } = null!;
    public long TotalTopics { get; set; }
    public int TotalTopicsPublished { get; set; }
    public long TotalStudentsEnrolled { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CourseSearch() { }
    public CourseSearch(
        Guid courseId,
        string title,
        string description,
        int orderIndex,
        int hskLevel,
        string slug,
        int totalTopicsPublished,
        long totalTopics,
        long totalStudentsEnrolled,
        bool isPublished,
        DateTime createdAt,
        DateTime updatedAt)
    {
        CourseId = courseId;
        Title = title;
        Description = description;
        OrderIndex = orderIndex;
        HskLevel = hskLevel;
        Slug = slug;
        TotalTopicsPublished = totalTopicsPublished;
        TotalTopics = totalTopics;
        TotalStudentsEnrolled = totalStudentsEnrolled;
        IsPublished = isPublished;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}