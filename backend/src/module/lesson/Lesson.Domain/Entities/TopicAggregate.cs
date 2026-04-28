namespace Lesson.Domain.Entities;

public enum TopicType
{
    Learning,
    Exam
}
public class TopicAggregate: BaseAggregateRoot
{
    public Guid TopicId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;  // URL-friendly slug
    public Guid CourseId { get; private set; }
    public TopicType TopicType { get; private set; }
    public long TotalExercises { get; private set; }
    public int TotalExercisesPublished { get; private set; }
    public int EstimatedTimeMinutes { get; private set; } // in minutes
    public int? ExamYear { get; private set; } // only for exam topics (nullable)
    public string ExamCode { get; private set; } = string.Empty; // only for exam topics For exam topics: code (e.g., "HSK1-2020-01")
    public int OrderIndex { get; private set; } // thứ tự hiển thị trong course
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsPublished { get; private set; }

    private TopicAggregate()
    {
    }

    [JsonConstructor] // Đánh dấu constructor này để System.Text.Json sử dụng khi deserialize từ JSON, đảm bảo các thuộc tính chỉ có getter vẫn được gán giá trị đúng cách khi đọc từ cache hoặc database.
    private TopicAggregate(
        Guid topicId,
        string title,
        string description,
        string slug,
        Guid courseId,
        TopicType topicType,
        long totalExercises,
        int totalExercisesPublished,
        int estimatedTimeMinutes,
        int? examYear,
        string examCode,
        int orderIndex,
        DateTime createdAt,
        DateTime updatedAt,
        bool isPublished)
    {
        TopicId = topicId;
        Title = title;
        Description = description;
        Slug = slug;
        CourseId = courseId;
        TopicType = topicType;
        TotalExercisesPublished = totalExercisesPublished;
        TotalExercises = totalExercises;
        EstimatedTimeMinutes = estimatedTimeMinutes;
        ExamYear = examYear;
        ExamCode = examCode;
        OrderIndex = orderIndex;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsPublished = isPublished;
    }

    public static TopicAggregate CreateTopic(
        string title,
        string description,
        Guid courseId,
        TopicType topicType,
        int estimatedTimeMinutes,
        int orderIndex,
        int? examYear = null,
        string examCode = "")
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không thể để trống.", nameof(title));
        if (courseId == Guid.Empty) throw new ArgumentException("CourseId không được để trống.", nameof(courseId));
        if (estimatedTimeMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(estimatedTimeMinutes), "Thời gian ước tính phải lớn hơn 0.");
        if (orderIndex < 1) throw new ArgumentOutOfRangeException(nameof(orderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");
        
        if (topicType == TopicType.Exam)
        {
            if (!examYear.HasValue || examYear <= 0) throw new ArgumentOutOfRangeException(nameof(examYear), "Năm thi phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(examCode)) throw new ArgumentException("ExamCode không được để trống cho chủ đề thi.", nameof(examCode));
        }

        var topic = new TopicAggregate
        {
            TopicId = Guid.CreateVersion7(),
            Title = title,
            Slug = "",  // Will be set below after object construction
            Description = description,
            CourseId = courseId,
            TopicType = topicType,
            EstimatedTimeMinutes = estimatedTimeMinutes,
            ExamYear = examYear,
            ExamCode = examCode,
            OrderIndex = orderIndex,
            TotalExercises = 0,
            TotalExercisesPublished = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false
        };
        topic.Slug = GenerateSlug(title); // Using inherited method
        topic.AddDomainEvent(new TopicCreatedEvent(
            topic.TopicId,
            topic.CourseId,
            topic.IsPublished,
            topic.Title,
            topic.Description,
            topic.Slug,
            topic.TopicType,
            topic.EstimatedTimeMinutes,
            topic.ExamYear,
            topic.ExamCode,
            topic.OrderIndex,
            topic.TotalExercises,
            topic.CreatedAt,
            topic.UpdatedAt,
            topic.TotalExercisesPublished
        ));
        return topic;
    }

    public void IncrementTotalExercisesPublished()
    {
        TotalExercisesPublished += 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicTotalExercisesPublishedUpdatedEvent(TopicId, TotalExercisesPublished, UpdatedAt));
    }
    public void DecrementTotalExercisesPublished()
    {
        if (TotalExercisesPublished <= 0)
            throw new InvalidOperationException("TotalExercisesPublished không thể nhỏ hơn 0.");

        TotalExercisesPublished -= 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicTotalExercisesPublishedUpdatedEvent(TopicId, TotalExercisesPublished, UpdatedAt));
    }
    public void Delete()
    {
        // check
        if (IsPublished)
            throw new InvalidOperationException("Không thể xóa chủ đề đã được xuất bản. Vui lòng hủy xuất bản trước khi xóa.");
        if(TotalExercises > 0)
            throw new InvalidOperationException("Không thể xóa chủ đề còn chứa bài tập. Vui lòng xóa hoặc chuyển các bài tập này trước khi xóa chủ đề.");
        AddDomainEvent(new TopicDeletedEvent(TopicId));
    }

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Tiêu đề không thể để trống.", nameof(newTitle));
        
        Title = newTitle;
        Slug = GenerateSlug(newTitle);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicTitleUpdatedEvent(TopicId, CourseId, newTitle, Slug, UpdatedAt));
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Mô tả không thể để trống.", nameof(newDescription));
        
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicDescriptionUpdatedEvent(TopicId, CourseId, newDescription, UpdatedAt));
    }

    public void UpdateEstimatedTime(int newEstimatedTimeMinutes)
    {
        if (newEstimatedTimeMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(newEstimatedTimeMinutes), "Thời gian ước tính phải lớn hơn 0.");
        
        EstimatedTimeMinutes = newEstimatedTimeMinutes;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicEstimatedTimeUpdatedEvent(TopicId, CourseId, newEstimatedTimeMinutes, UpdatedAt));
    }

    public void UpdateOrderIndex(int newOrderIndex)
    {
        if (newOrderIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(newOrderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");
        
        OrderIndex = newOrderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExamInfo(int newExamYear, string newExamCode)
    {
        if (TopicType != TopicType.Exam)
            throw new InvalidOperationException("Chỉ có thể cập nhật thông tin thi cho các chủ đề thi.");
        
        if (newExamYear <= 0)
            throw new ArgumentOutOfRangeException(nameof(newExamYear), "Năm thi phải lớn hơn 0.");
        
        if (string.IsNullOrWhiteSpace(newExamCode))
            throw new ArgumentException("ExamCode không được để trống.", nameof(newExamCode));
        
        ExamYear = newExamYear;
        ExamCode = newExamCode;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicExamInfoUpdatedEvent(TopicId, CourseId, newExamYear, newExamCode, UpdatedAt));
    }

    public void IncrementTotalExercises()
    {
        TotalExercises += 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicTotalExercisesUpdatedEvent(TopicId, TotalExercises, UpdatedAt));
    }

    public void DecrementTotalExercises()
    {
        if (TotalExercises <= 0)
            throw new InvalidOperationException("TotalExercises không thể nhỏ hơn 0.");

        TotalExercises -= 1;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicTotalExercisesUpdatedEvent(TopicId, TotalExercises, UpdatedAt));

    }

    public void Publish()
    {
        if (IsPublished) throw new InvalidOperationException("Chủ đề đã được xuất bản.");
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicPublishedEvent(
            TopicId,
            UpdatedAt
        ));
    }

    public void UnPublish()
    {
        if (!IsPublished) throw new InvalidOperationException("Chủ đề chưa được xuất bản.");
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicUnpublishedEvent(
            TopicId,
            UpdatedAt
        ));
    }
}