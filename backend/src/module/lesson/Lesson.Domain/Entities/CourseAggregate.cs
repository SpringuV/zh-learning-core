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

    private CourseAggregate(){}

    [JsonConstructor] // Đánh dấu constructor này để System.Text.Json sử dụng khi deserialize từ JSON, đảm bảo các thuộc tính chỉ có getter vẫn được gán giá trị đúng cách khi đọc từ cache hoặc database.
    private CourseAggregate(
        Guid courseId,
        string title,
        string slug,
        string description,
        int hskLevel,
        bool isPublished,
        DateTime createdAt,
        DateTime updatedAt,
        int orderIndex,
        long totalStudentsEnrolled,
        long totalTopics)
    {
        CourseId = courseId;
        Title = title;
        Slug = slug;
        Description = description;
        HskLevel = hskLevel;
        IsPublished = isPublished;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        OrderIndex = orderIndex;
        TotalStudentsEnrolled = totalStudentsEnrolled;
        TotalTopics = totalTopics;
    }

    public static CourseAggregate CreateCourse(string title, string description, int hskLevel, int orderIndex)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không thể để trống.", nameof(title));
        if (hskLevel < 0 || hskLevel > 6) throw new ArgumentOutOfRangeException(nameof(hskLevel), "Cấp HSK phải từ 0 đến 6.");
        if (orderIndex < 1) throw new ArgumentOutOfRangeException(nameof(orderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");
        var course = new CourseAggregate
        {
            CourseId = Guid.NewGuid(),
            Title = title,
            Slug = GenerateSlug(title),
            Description = description,
            HskLevel = hskLevel,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Fire domain event — các handler khác lắng nghe
        course.AddDomainEvent(new CourseCreatedEvent(
            course.CourseId, 
            course.Title,
            course.Description,
            course.HskLevel,
            course.OrderIndex,
            course.Slug,
            0, // TotalStudentsEnrolled
            0, // TotalTopics
            false, // IsPublished
            course.CreatedAt,
            course.UpdatedAt));

        return course;
    }
    
    public void Delete()
    {
        if (IsPublished || TotalStudentsEnrolled > 0 || TotalTopics > 0)
            throw new InvalidOperationException("Không thể xóa khóa học đã được xuất bản hoặc đã có học viên hoặc chủ đề.");
        AddDomainEvent(new CourseDeletedEvent(CourseId));
    }

    /// <summary>
    /// Update course title & regenerate slug
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Tiêu đề không thể để trống.", nameof(newTitle));
        Title = newTitle;
        Slug =GenerateSlug(newTitle);  // Using inherited method
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CourseTitleUpdatedEvent(CourseId, newTitle, Slug, UpdatedAt));
    }

    public void UpdateOrderIndex(int newOrderIndex)
    {
        if (newOrderIndex < 1) throw new ArgumentOutOfRangeException(nameof(newOrderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");
        OrderIndex = newOrderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Mô tả không được để trống.", nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseDescriptionUpdatedEvent(CourseId, newDescription, UpdatedAt));
    }

    public void UpdateHskLevel(int newHskLevel)
    {
        if (newHskLevel < 0 || newHskLevel > 9)
            throw new ArgumentOutOfRangeException(nameof(newHskLevel), "Cấp HSK phải từ 0 đến 9.");

        if (HskLevel == newHskLevel)
            return;

        if (IsPublished && TotalStudentsEnrolled > 0)
            throw new InvalidOperationException("Không thể cập nhật cấp HSK khi khóa học đã được xuất bản và đã có học viên.");

        HskLevel = newHskLevel;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseHskLevelUpdatedEvent(CourseId, newHskLevel, UpdatedAt));
    }

    public void IncrementEnrollmentCount()
    {
        TotalStudentsEnrolled++;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseEnrollmentCountUpdatedEvent(CourseId, TotalStudentsEnrolled, UpdatedAt));
    }

    public void IncrementTotalTopics()
    {
        TotalTopics += 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseTotalTopicsUpdatedEvent(CourseId, TotalTopics, UpdatedAt));
    }

    public void DecrementTotalTopics()
    {
        if (TotalTopics <= 0)
            throw new InvalidOperationException("TotalTopics không thể nhỏ hơn 0.");

        TotalTopics -= 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CourseTotalTopicsUpdatedEvent(CourseId, TotalTopics, UpdatedAt));
    }

    public void Publish()
    {
        if (IsPublished) throw new InvalidOperationException("Khóa học đã được xuất bản.");
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;

        // Fire domain event
        AddDomainEvent(new CoursePublishedDomainEvent(CourseId, UpdatedAt));
    }

    public void UnPublish()
    {
        if (!IsPublished) throw new InvalidOperationException("Khóa học chưa được xuất bản.");
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;

        // Fire domain event
        AddDomainEvent(new CourseUnpublishedDomainEvent(CourseId, UpdatedAt));
    }

}

