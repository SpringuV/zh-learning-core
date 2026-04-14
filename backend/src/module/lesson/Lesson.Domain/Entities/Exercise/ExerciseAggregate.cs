namespace Lesson.Domain.Entities.Exercise;
public class ExerciseAggregate: BaseAggregateRoot
{
    public Guid ExerciseId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid TopicId { get; private set; }
    public int OrderIndex { get; private set; } // thứ tự hiển thị trong topic
    public DateTime CreatedAt { get; private set; }
    public string Slug { get; private set; } = string.Empty;
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
    private readonly List<ExerciseOption> _options = []; // chỉ dùng cho các bài tập có lựa chọn (choice, match, order), không dùng cho fill blank, write sentence, v.v.
    public IReadOnlyList<ExerciseOption> Options => _options.AsReadOnly();
    
    public ExerciseAggregate() {}
    private ExerciseAggregate(List<ExerciseOption>? options)
    {
        _options = options ?? [];
    }
    #region Helpers
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
    public void ValidateOptions()
    {
        if (!HasOptions() && OptionTypesRequireOptions())
            throw new InvalidOperationException(
                $"{ExerciseType} requires at least one option. Please provide options when creating this exercise type.");
        
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
    public bool HasOptions() => _options.Count > 0;

    public static bool RequiresOptions(ExerciseType exerciseType)
    {
        return exerciseType == ExerciseType.ListenDialogueChoice ||
               exerciseType == ExerciseType.ListenSentenceJudge ||
               exerciseType == ExerciseType.ReadComprehension ||
               exerciseType == ExerciseType.ReadMatch ||
               exerciseType == ExerciseType.ListenImageChoice ||
               exerciseType == ExerciseType.ReadSentenceOrder;
    }

    // Kiểm tra nếu ExerciseType là loại bài tập yêu cầu có options thì bắt buộc phải
    // có ít nhất một option, nếu không có options thì sẽ không hợp lệ.
    private bool OptionTypesRequireOptions()
    {
        return RequiresOptions(ExerciseType);
    }

    // Lấy option đúng dựa trên CorrectAnswer, nếu không tìm thấy thì ném lỗi 
    // vì cấu trúc dữ liệu không hợp lệ (đáp án đúng phải tồn tại trong options).
    public ExerciseOption GetCorrectOption()
    {
        var correct = _options.FirstOrDefault(o => IsOptionCorrect(o.Id)) ?? throw new InvalidOperationException($"Không tìm thấy lựa chọn đúng: '{CorrectAnswer}'");
        return correct;
    }
    public IReadOnlyList<ExerciseOption> GetIncorrectOptions() =>
        [.. _options.Where(o => !IsOptionCorrect(o.Id))];
    #endregion
    #region Create
    public static ExerciseAggregate CreateExercise(
        Guid topicId,
        string description,
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
        
        // nếu loại bài tập yêu cầu có options mà không có options hoặc options rỗng thì ném lỗi
        if (RequiresOptions(exerciseType) && (options == null || options.Count == 0))
            throw new ArgumentException($"{exerciseType} requires at least one option. Please provide options when creating this exercise type.", nameof(options));
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
            IsPublished = false,
        };

        // Enforce option rules ngay từ bước create để tránh tạo draft sai cấu trúc.
        exercise.ValidateOptions();

        exercise.Slug = GenerateSlug($"{exercise.ExerciseType}-{exercise.Question[..Math.Min(20, exercise.Question.Length)]}");
        
        exercise.AddDomainEvent(new ExerciseCreatedEvent(
            exercise.ExerciseId,
            exercise.TopicId,
            exercise.Description,
            exercise.OrderIndex,
            exercise.CreatedAt,
            exercise.UpdatedAt,
            exercise.IsPublished,
            exercise.ExerciseType,
            exercise.SkillType,
            exercise.Question,
            exercise.CorrectAnswer,
            exercise.Difficulty,
            exercise.Context,
            exercise.AudioUrl,
            exercise.ImageUrl,
            exercise.Explanation,
            exercise.Slug,
            [.. exercise.Options]
        ));
        
        return exercise;
    }
    #endregion
    #region Publish/Unpublish
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
    #endregion

    #region Update
    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Description cannot be empty", nameof(newDescription));
        
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseDescriptionUpdatedEvent(ExerciseId, newDescription, UpdatedAt));
    }

    public void UpdateOrderIndex(int newOrderIndex)
    {
        if (newOrderIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(newOrderIndex), "OrderIndex must be greater than 0");
        
        OrderIndex = newOrderIndex;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseOrderIndexUpdatedEvent(ExerciseId, newOrderIndex, UpdatedAt));
    }

    public void UpdateQuestion(string newQuestion)
    {
        if (string.IsNullOrWhiteSpace(newQuestion))
            throw new ArgumentException("Question cannot be empty", nameof(newQuestion));
        
        Question = newQuestion;
        Slug = GenerateSlug($"{ExerciseType}-{newQuestion[..Math.Min(20, newQuestion.Length)]}");
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseQuestionUpdatedEvent(ExerciseId, newQuestion, Slug, UpdatedAt));
    }

    public void UpdateCorrectAnswer(string newCorrectAnswer)
    {
        if (string.IsNullOrWhiteSpace(newCorrectAnswer))
            throw new ArgumentException("CorrectAnswer cannot be empty", nameof(newCorrectAnswer));
        
        CorrectAnswer = newCorrectAnswer;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseCorrectAnswerUpdatedEvent(ExerciseId, newCorrectAnswer, UpdatedAt));
    }

    public void UpdateDifficulty(ExerciseDifficulty newDifficulty)
    {
        Difficulty = newDifficulty;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseDifficultyUpdatedEvent(ExerciseId, newDifficulty, UpdatedAt));
    }

    public void UpdateContext(ExerciseContext newContext)
    {
        Context = newContext;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseContextUpdatedEvent(ExerciseId, newContext, UpdatedAt));
    }

    public void UpdateAudioUrl(string newAudioUrl)
    {
        if (string.IsNullOrWhiteSpace(newAudioUrl))
            throw new ArgumentException("AudioUrl cannot be empty.", nameof(newAudioUrl));
        AudioUrl = newAudioUrl ;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseAudioUrlUpdatedEvent(ExerciseId, newAudioUrl, UpdatedAt));
    }

    public void UpdateImageUrl(string newImageUrl)
    {
        if (string.IsNullOrWhiteSpace(newImageUrl))
            throw new ArgumentException("ImageUrl cannot be empty.", nameof(newImageUrl));
        ImageUrl = newImageUrl ;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseImageUrlUpdatedEvent(ExerciseId, newImageUrl, UpdatedAt));
    }

    public void UpdateExplanation(string newExplanation)
    {
        if (string.IsNullOrWhiteSpace(newExplanation))
            throw new ArgumentException("Explanation cannot be empty.", nameof(newExplanation));
        Explanation = newExplanation;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ExerciseExplanationUpdatedEvent(ExerciseId, newExplanation, UpdatedAt));
    }

    public void UpdateOptions(List<ExerciseOption> newOptions)
    {
        _options.Clear();
        if (newOptions != null)
        {
            _options.AddRange(newOptions);
        }
        UpdatedAt = DateTime.UtcNow;
        ValidateOptions();
        AddDomainEvent(new ExerciseOptionsUpdatedEvent(ExerciseId, [.. _options], UpdatedAt));
    }
    #endregion
}
   