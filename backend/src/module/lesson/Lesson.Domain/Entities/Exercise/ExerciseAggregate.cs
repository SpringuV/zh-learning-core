using System.Text.Json.Serialization;
using HanziAnhVu.Shared.Domain;
using Lesson.Domain.Entities.Events;

namespace Lesson.Domain.Entities.Exercise;

public enum ExerciseType
{
    ListenDialogueChoice, // Input: Audio + Multiple Choice Question
    ListenFillBlank, // Input: Audio + Fill in the Blank
    ListenSentenceJudge, // Input: Audio + True/False Question
    ReadFillBlank, // Input: Text + Fill in the Blank
    ReadComprehension, // Input: Text + Multiple Choice Question, Read passage, answer questions
    ReadSentenceOrder, // Input: Text + Sentence Ordering Question, Order sentences correctly
    ReadMatch, // Input: Text + Matching Question, Match phrases or definitions
    WriteHanzi, // Input: Text + Hanzi Writing, Write Chinese character (stroke order)
    WritePinyin, // Input: Text + Pinyin Writing,  Write pinyin for character
    WriteSentence, // Input: Text + Sentence Writing,Write complete sentence (AI feedback)
}
public enum SkillType
{
    Reading,
    Writing,
    Listening,
    Speaking
}

public enum ExerciseDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum ExerciseContext
{
    Learning,   // Dành cho topic-based learning (self-paced)
    Classroom,  // Dành cho assignment giao bài trong lớp
    Mixed       // Có thể dùng cho cả hai
}

// Value Object cho option
// ExerciseOption sẽ được lưu trực tiếp trong ExerciseAggregate dưới dạng JSON, 
// không có bảng riêng
// cái option này có tác dụng là để lưu trữ các lựa chọn cho bài tập dạng multiple choice, 
// hoặc các câu trả lời đúng/sai cho bài tập dạng true/false, v.v.
//  ReadSentenceOrder
//  ReadMatch (Matching pairs)
//  ListenSentenceJudge
//  ListenDialogueChoice
/// <summary>
/// Value Object for Exercise Options
/// Used for multiple choice, matching, ordering exercises, etc.
/// IsCorrect is NOT stored here - it's derived from Aggregate's CorrectAnswer field
/// </summary>
public class ExerciseOption : ValueObject
{
    public string Id { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    
    [JsonConstructor]
    public ExerciseOption() { }
    
    public ExerciseOption(string id, string text)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Option Id cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option Text cannot be empty", nameof(text));
        
        Id = id;
        Text = text;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Text;
    }
}

public class ExerciseAggregate: BaseAggregateRoot
{
    public Guid ExerciseId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid TopicId { get; private set; }
    public int OrderIndex { get; private set; } // thứ tự hiển thị trong topic
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsPublished { get; private set; }
    public ExerciseType ExerciseType { get; private set; }
    public SkillType SkillType { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string CorrectAnswer { get; private set; } = string.Empty;
    public ExerciseDifficulty Difficulty { get; private set; }
    public ExerciseContext Context { get; private set; } = ExerciseContext.Learning; // learning | classroom | mixed
    public string AudioUrl { get; private set; } = string.Empty; // chỉ dùng cho bài tập nghe
    public string ImageUrl { get; private set; } = string.Empty; // dùng cho bài tập có hình ảnh minh họa
    public string Explanation { get; private set; } = string.Empty; // giải thích đáp án, dùng cho tất cả loại bài tập
    private readonly List<ExerciseOption> _options = [];
    public IReadOnlyList<ExerciseOption> Options => _options.AsReadOnly();
    
    public ExerciseAggregate() {}
    
    /// <summary>
    /// Constructor for initializing with options
    /// </summary>
    private ExerciseAggregate(List<ExerciseOption>? options)
    {
        _options = options ?? [];
    }
    
    /// <summary>
    /// Check if an option is the correct answer
    /// </summary>
    public bool IsOptionCorrect(string optionId) => CorrectAnswer == optionId;
    
    /// <summary>
    /// Add an option (for multiple choice, matching, ordering exercises)
    /// </summary>
    public void AddOption(string id, string text)
    {
        var option = new ExerciseOption(id, text);
        _options.Add(option);
    }
    
    /// <summary>
    /// Remove an option by ID
    /// </summary>
    public void RemoveOption(string id)
    {
        var option = _options.FirstOrDefault(o => o.Id == id);
        if (option is null)
        {
            return;
        }
        _options.Remove(option);
    }
    
    /// <summary>
    /// Validate that options are correctly configured for this exercise type
    /// Called before publishing
    /// </summary>
    public void ValidateOptions()
    {
        if (!HasOptions() && OptionTypesRequireOptions())
            throw new InvalidOperationException(
                $"{ExerciseType} requires at least one option. Please add options before publishing.");
        
        if (HasOptions() && !OptionTypesRequireOptions())
            throw new InvalidOperationException(
                $"{ExerciseType} does not support options. Please remove options.");
        
        // Validate correct answer exists in options (if options exist)
        if (HasOptions())
        {
            var correctOptionExists = _options.Any(o => IsOptionCorrect(o.Id));
            if (!correctOptionExists)
                throw new InvalidOperationException(
                    $"Đáp án đúng '{CorrectAnswer}' không tồn tại trong các lựa chọn. Vui lòng chọn một lựa chọn hợp lệ làm đáp án đúng.");
        }
    }
    
    /// <summary>
    /// Check if exercise has options
    /// </summary>
    public bool HasOptions() => _options.Count > 0;
    
    /// <summary>
    /// Check if this exercise type requires options
    /// </summary>
    private bool OptionTypesRequireOptions()
    {
        return ExerciseType == ExerciseType.ListenDialogueChoice ||
               ExerciseType == ExerciseType.ListenSentenceJudge ||
               ExerciseType == ExerciseType.ReadComprehension ||
               ExerciseType == ExerciseType.ReadMatch ||
               ExerciseType == ExerciseType.ReadSentenceOrder;
    }
    
    /// <summary>
    /// Get the correct option
    /// </summary>
    public ExerciseOption GetCorrectOption()
    {
        var correct = _options.FirstOrDefault(o => IsOptionCorrect(o.Id)) ?? throw new InvalidOperationException($"Không tìm thấy lựa chọn đúng: '{CorrectAnswer}'");
        return correct;
    }
    
    /// <summary>
    /// Get all incorrect options
    /// </summary>
    public IReadOnlyList<ExerciseOption> GetIncorrectOptions() =>
        [.. _options.Where(o => !IsOptionCorrect(o.Id))];

    public static ExerciseAggregate Create(
        string description,
        Guid topicId,
        int orderIndex,
        ExerciseType exerciseType,
        SkillType skillType,
        string question,
        string correctAnswer,
        ExerciseDifficulty difficulty,
        ExerciseContext context = ExerciseContext.Learning,
        string audioUrl = "",
        string imageUrl = "",
        string explanation = "",
        List<ExerciseOption>? options = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId cannot be empty", nameof(topicId));
        if (orderIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(orderIndex), "OrderIndex must be greater than 0");
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question cannot be empty", nameof(question));
        if (string.IsNullOrWhiteSpace(correctAnswer))
            throw new ArgumentException("CorrectAnswer cannot be empty", nameof(correctAnswer));
        
        var exercise = new ExerciseAggregate(options)
        {
            ExerciseId = Guid.CreateVersion7(),
            Description = description,
            TopicId = topicId,
            OrderIndex = orderIndex,
            ExerciseType = exerciseType,
            SkillType = skillType,
            Question = question,
            CorrectAnswer = correctAnswer,
            Difficulty = difficulty,
            Context = context,
            AudioUrl = audioUrl,
            ImageUrl = imageUrl,
            Explanation = explanation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false
        };
        
        exercise.AddDomainEvent(new ExerciseCreatedEvent(
            exercise.ExerciseId,
            exercise.TopicId,
            exercise.Description,
            exercise.OrderIndex,
            exercise.CreatedAt,
            exercise.UpdatedAt,
            exercise.IsPublished,
            exercise.ExerciseType.ToString(),
            exercise.SkillType.ToString(),
            exercise.Question,
            exercise.CorrectAnswer,
            exercise.Difficulty.ToString(),
            exercise.Context.ToString(),
            exercise.AudioUrl,
            exercise.ImageUrl,
            exercise.Explanation,
            [.. exercise.Options]
        ));
        
        return exercise;
    }
    
    /// <summary>
    /// Publish exercise (make it visible to students)
    /// Validates that all required data is present
    /// </summary>
    public void Publish()
    {
        if (IsPublished)
            throw new InvalidOperationException("Exercise đã được xuất bản.");
        
        // Validate options if needed
        ValidateOptions();
        
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
        
        // Publish event (minimal: just key for partial update)
        AddDomainEvent(new ExercisePublishedEvent(
            ExerciseId,
            UpdatedAt
        ));
    }
    
    /// <summary>
    /// Unpublish exercise
    /// </summary>
    public void Unpublish()
    {
        if (!IsPublished)
            throw new InvalidOperationException("Exercise chưa được xuất bản.");
        
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseUnpublishedEvent(
            ExerciseId,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Update exercise with optional fields (bulk update)
    /// Only provided fields will be updated
    /// </summary>
    public void Update(
        string? description = null,
        int? orderIndex = null,
        string? question = null,
        string? correctAnswer = null,
        ExerciseDifficulty? difficulty = null,
        ExerciseContext? context = null,
        string? audioUrl = null,
        string? imageUrl = null,
        string? explanation = null,
        List<ExerciseOption>? options = null)
    {
        if (IsPublished)
            throw new InvalidOperationException("Không thể cập nhật bài tập đã xuất bản.");
        
        // Track which fields changed
        var hasChanges = false;
        
        if (description is not null && Description != description)
        {
            Description = description;
            hasChanges = true;
        }
        
        if (orderIndex is not null && OrderIndex != orderIndex)
        {
            if (orderIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(orderIndex), "OrderIndex phải > 0");
            OrderIndex = orderIndex.Value;
            hasChanges = true;
        }
        
        if (question is not null && Question != question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Câu hỏi không được để trống", nameof(question));
            Question = question;
            hasChanges = true;
        }
        
        if (correctAnswer is not null && CorrectAnswer != correctAnswer)
        {
            if (string.IsNullOrWhiteSpace(correctAnswer))
                throw new ArgumentException("Câu trả lời đúng không được để trống", nameof(correctAnswer));
            CorrectAnswer = correctAnswer;
            hasChanges = true;
        }
        
        if (difficulty.HasValue && Difficulty != difficulty.Value)
        {
            Difficulty = difficulty.Value;
            hasChanges = true;
        }
        
        if (context.HasValue && Context != context.Value)
        {
            Context = context.Value;
            hasChanges = true;
        }
        
        if (audioUrl is not null && AudioUrl != audioUrl)
        {
            AudioUrl = audioUrl;
            hasChanges = true;
        }
        
        if (imageUrl is not null && ImageUrl != imageUrl)
        {
            ImageUrl = imageUrl;
            hasChanges = true;
        }
        
        if (explanation is not null && Explanation != explanation)
        {
            Explanation = explanation;
            hasChanges = true;
        }
        
        if (options is not null)
        {
            _options.Clear();
            _options.AddRange(options);
            hasChanges = true;
        }
        
        // Only emit event if something actually changed
        if (hasChanges)
        {
            UpdatedAt = DateTime.UtcNow;
            ValidateOptions();
            
            AddDomainEvent(new ExerciseUpdatedEvent(
                ExerciseId,
                TopicId,
                description,
                orderIndex,
                UpdatedAt,
                question,
                correctAnswer,
                difficulty?.ToString(),
                context?.ToString(),
                audioUrl,
                imageUrl,
                explanation,
                options
            ));
        }
    }
}
   