
using HanziAnhVu.Shared.Domain;
using Lesson.Domain.Entities.Events;

namespace Lesson.Domain.Entities;

public class CourseAggregate : BaseAggregateRoot
{
    public Guid CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;  // URL-friendly slug
    public string Description { get; private set; } = string.Empty;
    public int HskLevel { get; private set; } = 0;
    public bool IsPublished { get; private set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public int OrderIndex { get; private set; } = 1;
    public long TotalStudentsEnrolled { get; private set; } = 0;
    public long TotalTopics { get; private set; } = 0;
 
    private readonly List<Guid> _TopicIds = [];
    public IReadOnlyList<Guid> TopicIds => _TopicIds.AsReadOnly();

    public CourseAggregate(){}

    public static CourseAggregate CreateCourse(string title, string description, int hskLevel, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không thể để trống.", nameof(title));
        if (hskLevel < 0 || hskLevel > 6) throw new ArgumentOutOfRangeException(nameof(hskLevel), "Cấp HSK phải từ 0 đến 6.");
        if (orderIndex < 1) throw new ArgumentOutOfRangeException(nameof(orderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");

        var course = new CourseAggregate
        {
            CourseId = Guid.NewGuid(),
            Title = title,
            Slug = "",  // Will be set below after object construction
            Description = description,
            HskLevel = hskLevel,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Generate slug using inherited method
        course.Slug = course.GenerateSlug(title);

        // Fire domain event — các handler khác lắng nghe
        course.AddDomainEvent(
            new CourseCreatedEvent(
                course.CourseId, 
                course.Title, 
                course.Description,
                course.HskLevel,
                course.OrderIndex,
                course.CreatedAt,
                course.UpdatedAt,
                course.Slug
            ));

        return course;
    }
    
    /// <summary>
    /// Update course title & regenerate slug
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Tiêu đề không thể để trống.", nameof(newTitle));
        
        Title = newTitle;
        Slug = GenerateSlug(newTitle);  // Using inherited method
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CourseTitleUpdatedEvent(CourseId, newTitle, Slug, UpdatedAt));
    }

    public void IncrementEnrollmentCount()
    {
        TotalStudentsEnrolled++;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseEnrollmentCountUpdatedEvent(CourseId, TotalStudentsEnrolled, UpdatedAt));
    }

    public void Publish()
    {
        if (IsPublished) throw new InvalidOperationException("Khóa học đã được xuất bản.");
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;

        // Fire domain event
        AddDomainEvent(new CoursePublishedDomainEvent(CourseId, UpdatedAt));
    }

    public void Unpublish()
    {
        if (!IsPublished) throw new InvalidOperationException("Khóa học chưa được xuất bản.");
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;

        // Fire domain event
        AddDomainEvent(new CourseUnpublishedDomainEvent(CourseId, UpdatedAt));
    }

    public void AddTopic(Guid topicId, int orderIndex)
    {
        if (_TopicIds.Contains(topicId)) throw new InvalidOperationException("Chủ đề đã tồn tại trong khóa học.");
        _TopicIds.Add(topicId);
        TotalTopics = _TopicIds.Count;
        UpdatedAt = DateTime.UtcNow;
        OrderIndex = orderIndex;
        // Fire domain event
        AddDomainEvent(new TopicAddedToCourseDomainEvent(CourseId, topicId, OrderIndex, UpdatedAt));
    }
}
