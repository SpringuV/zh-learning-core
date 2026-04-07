using HanziAnhVu.Shared.Domain;
using Lesson.Domain.Entities.Events;

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
    public int EstimatedTimeMinutes { get; private set; } // in minutes
    public int? ExamYear { get; private set; } // only for exam topics (nullable)
    public string ExamCode { get; private set; } = string.Empty; // only for exam topics For exam topics: code (e.g., "HSK1-2020-01")
    public int OrderIndex { get; private set; } // thứ tự hiển thị trong course
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsPublished { get; private set; }

    private readonly List<Guid> _ExerciseIds = [];
    public IReadOnlyList<Guid> ExerciseIds => _ExerciseIds.AsReadOnly();

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false
        };
        topic.Slug = topic.GenerateSlug(title); // Using inherited method
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
            topic.CreatedAt,
            topic.UpdatedAt
        ));
        return topic;
    }

    public void UpdateTopic(
        string? title,
        string? description,
        TopicType? topicType,
        int? estimatedTimeMinutes,
        int? orderIndex,
        int? examYear = null,
        string? examCode = "")
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề không thể để trống.", nameof(title));
        if (estimatedTimeMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(estimatedTimeMinutes), "Thời gian ước tính phải lớn hơn 0.");
        if (orderIndex < 1) throw new ArgumentOutOfRangeException(nameof(orderIndex), "Chỉ số thứ tự phải lớn hơn hoặc bằng 1.");
        
        if (topicType == TopicType.Exam)
        {
            if (!examYear.HasValue || examYear <= 0) throw new ArgumentOutOfRangeException(nameof(examYear), "Năm thi phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(examCode)) throw new ArgumentException("ExamCode không được để trống cho chủ đề thi.", nameof(examCode));
        }

        if (!string.IsNullOrWhiteSpace(title))
        { 
            Title = title;
            Slug = GenerateSlug(title);
        }
        if (!string.IsNullOrWhiteSpace(description))
        {
            Description = description;
        }
        if (topicType.HasValue)
        {
            TopicType = topicType.Value;
        }
        if (estimatedTimeMinutes.HasValue)
        {
            EstimatedTimeMinutes = estimatedTimeMinutes.Value;
        }
        if (examYear.HasValue)
        {
            ExamYear = examYear.Value;
        }
        if (!string.IsNullOrWhiteSpace(examCode))
        {
            ExamCode = examCode;
        }
        if (orderIndex.HasValue)
        {
            OrderIndex = orderIndex.Value;
        }
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TopicUpdatedEvent(
            TopicId,
            CourseId,
            IsPublished,
            Title,
            Description,
            Slug,
            TopicType,
            EstimatedTimeMinutes,
            ExamYear,
            ExamCode,
            OrderIndex,
            UpdatedAt
        ));
    }

    public void AddExercise(Guid exerciseId)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId không được để trống.", nameof(exerciseId));
        if (_ExerciseIds.Contains(exerciseId)) throw new InvalidOperationException("Exercise đã tồn tại trong chủ đề.");
        
        _ExerciseIds.Add(exerciseId);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseAddedToTopicEvent(
            TopicId,
            exerciseId,
            UpdatedAt
        ));
    }
    public void RemoveExercise(Guid exerciseId)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId không được để trống.", nameof(exerciseId));
        if (!_ExerciseIds.Contains(exerciseId)) throw new InvalidOperationException("Exercise không tồn tại trong chủ đề.");
        
        _ExerciseIds.Remove(exerciseId);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseRemovedFromTopicEvent(
            TopicId,
            exerciseId,
            UpdatedAt
        ));
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

    public void Unpublish()
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