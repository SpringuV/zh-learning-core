namespace Search.Domain.Entities;

/// <summary>
/// AssignmentSearch entity for Elasticsearch indexing
/// Denormalized and enriched document for fast searching
/// </summary>
public class AssignmentSearch
{
    // Denormalized from Classroom.AssignmentAggregate
    public string Id { get; set; } = string.Empty;
    public Guid ClassroomId { get; set; }
    public Guid TeacherId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssignmentType { get; set; } = string.Empty;  // AllClass, Individual
    public string SkillFocus { get; set; } = string.Empty;      // Reading, Writing, Listening, Speaking
    public DateTime DueDate { get; set; }
    public bool IsPublished { get; set; }
    
    // Enriched counts from child entities
    public int ExerciseCount { get; set; }
    public int RecipientCount { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AssignmentSearch() { }

    public AssignmentSearch(
        string id,
        Guid classroomId,
        Guid teacherId,
        string title,
        string description,
        string assignmentType,
        string skillFocus,
        DateTime dueDate,
        bool isPublished,
        int exerciseCount,
        int recipientCount,
        DateTime createdAt,
        DateTime updatedAt)
    {
        this.Id = id;
        this.ClassroomId = classroomId;
        this.TeacherId = teacherId;
        this.Title = title;
        this.Description = description;
        this.AssignmentType = assignmentType;
        this.SkillFocus = skillFocus;
        this.DueDate = dueDate;
        this.IsPublished = isPublished;
        this.ExerciseCount = exerciseCount;
        this.RecipientCount = recipientCount;
        this.CreatedAt = createdAt;
        this.UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Factory method to create AssignmentSearch from assignment data
    /// Typically called when assignment is published or created
    /// </summary>
    public static AssignmentSearch FromAssignment(
        Guid assignmentId,
        Guid classroomId,
        Guid teacherId,
        string title,
        string description,
        string assignmentType,
        string skillFocus,
        DateTime dueDate,
        bool isPublished,
        int exerciseCount,
        int recipientCount,
        DateTime createdAt,
        DateTime updatedAt)
    {
        ValidateTitle(title);
        ValidateDueDate(dueDate);
        ValidateExerciseCount(exerciseCount);

        return new AssignmentSearch
        {
            Id = assignmentId.ToString(),
            ClassroomId = classroomId,
            TeacherId = teacherId,
            Title = title,
            Description = description,
            AssignmentType = assignmentType,
            SkillFocus = skillFocus,
            DueDate = dueDate,
            IsPublished = isPublished,
            ExerciseCount = exerciseCount,
            RecipientCount = recipientCount,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void UpdateTitle(string newTitle, DateTime updatedAt)
    {
        ValidateTitle(newTitle);
        Title = newTitle;
        UpdatedAt = updatedAt;
    }

    public void UpdateDescription(string newDescription, DateTime updatedAt)
    {
        Description = newDescription;
        UpdatedAt = updatedAt;
    }

    public void UpdateDueDate(DateTime newDueDate, DateTime updatedAt)
    {
        ValidateDueDate(newDueDate);
        DueDate = newDueDate;
        UpdatedAt = updatedAt;
    }

    public void UpdatePublished(bool isPublished, DateTime updatedAt)
    {
        IsPublished = isPublished;
        UpdatedAt = updatedAt;
    }

    public void UpdateSkillFocus(string skillFocus, DateTime updatedAt)
    {
        SkillFocus = skillFocus;
        UpdatedAt = updatedAt;
    }

    public void UpdateExerciseCount(int count, DateTime updatedAt)
    {
        ValidateExerciseCount(count);
        ExerciseCount = count;
        UpdatedAt = updatedAt;
    }

    public void UpdateRecipientCount(int count, DateTime updatedAt)
    {
        if (count < 0)
            throw new ArgumentException("Số lượng người nhận không thể là số âm.", nameof(count));

        RecipientCount = count;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Validates assignment title
    /// </summary>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề bài tập không thể để trống.", nameof(title));

        if (title.Length > 500)
            throw new ArgumentException("Tiêu đề bài tập không thể vượt quá 500 ký tự.", nameof(title));
    }

    /// <summary>
    /// Validates due date
    /// </summary>
    private static void ValidateDueDate(DateTime dueDate)
    {
        if (dueDate < DateTime.UtcNow)
            throw new ArgumentException("Ngày hết hạn không thể ở trong quá khứ.", nameof(dueDate));
    }

    /// <summary>
    /// Validates exercise count
    /// </summary>
    private static void ValidateExerciseCount(int count)
    {
        if (count < 1)
            throw new ArgumentException("Bài tập phải chứa ít nhất 1 bài tập.", nameof(count));
    }
}
