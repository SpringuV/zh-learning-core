using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities.Assignment;
public enum SkillFocus
{
    Reading,
    Writing,
    Listening,
    Speaking
}
public enum AssignmentType
{
    AllClass,     // Giao bài tập cho toàn bộ lớp học
    Individual    // Giao bài tập cho từng học viên cụ thể
}

/// <summary>
/// AssignmentAggregate - Assignment with exercises and recipients
/// Child entities: AssignmentExercise[], AssignmentRecipient[] (only for Individual type)
/// </summary>
public class AssignmentAggregate : BaseAggregateRoot
{
    public Guid AssignmentId { get; private set; }
    public Guid ClassroomId { get; private set; }
    public Guid TeacherId { get; private set; } // Soft ref → users.id (cross-module)
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AssignmentType AssignmentType { get; private set; } = AssignmentType.AllClass;
    public SkillFocus SkillFocus { get; private set; } = SkillFocus.Reading;
    public DateTime DueDate { get; private set; } // Ngày hết hạn nộp bài
    public bool IsPublished { get; private set; } = false; // Trạng thái bài tập đã được giáo viên công bố hay chưa
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Child entities collections
    private readonly List<AssignmentExercise> _exercises = [];
    public IReadOnlyList<AssignmentExercise> Exercises => _exercises.AsReadOnly();

    private readonly List<AssignmentRecipient> _recipients = [];
    public IReadOnlyList<AssignmentRecipient> Recipients => _recipients.AsReadOnly();

    public static AssignmentAggregate Create(
        Guid classroomId,
        Guid teacherId,
        string title,
        string description,
        AssignmentType assignmentType,
        SkillFocus skillFocus,
        DateTime dueDate)
    {
        if (classroomId == Guid.Empty)
            throw new ArgumentException("ClassroomId không được để trống.", nameof(classroomId));
        if (teacherId == Guid.Empty)
            throw new ArgumentException("TeacherId không được để trống.", nameof(teacherId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống.", nameof(title));
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Ngày hết hạn phải lớn hơn ngày hiện tại.", nameof(dueDate));

        var assignment = new AssignmentAggregate
        {
            AssignmentId = Guid.CreateVersion7(),
            ClassroomId = classroomId,
            TeacherId = teacherId,
            Title = title,
            Description = description,
            AssignmentType = assignmentType,
            SkillFocus = skillFocus,
            DueDate = dueDate,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return assignment;
    }

    /// <summary>
    /// Add exercise to assignment
    /// </summary>
    public void AddExercise(Guid exerciseId, int orderIndex)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot add exercise to published assignment.");

        var exercise = AssignmentExercise.Create(AssignmentId, exerciseId, orderIndex);
        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove exercise from assignment
    /// </summary>
    public void RemoveExercise(Guid exerciseId)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot remove exercise from published assignment.");

        var exercise = _exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
        if (exercise != null)
        {
            _exercises.Remove(exercise);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Add recipient for Individual assignment type
    /// </summary>
    public void AddRecipient(Guid studentId)
    {
        if (AssignmentType != AssignmentType.Individual)
            throw new InvalidOperationException("Recipients can only be added to Individual assignment type.");

        var recipient = AssignmentRecipient.Create(AssignmentId, studentId);
        _recipients.Add(recipient);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove recipient from Individual assignment
    /// </summary>
    public void RemoveRecipient(Guid studentId)
    {
        var recipient = _recipients.FirstOrDefault(r => r.StudentId == studentId);
        if (recipient != null)
        {
            _recipients.Remove(recipient);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Publish assignment (make visible to students)
    /// </summary>
    public void Publish()
    {
        if (IsPublished)
            throw new InvalidOperationException("Assignment is already published.");
        if (_exercises.Count == 0)
            throw new InvalidOperationException("Cannot publish assignment without exercises.");
        if (AssignmentType == AssignmentType.Individual && _recipients.Count == 0)
            throw new InvalidOperationException("Cannot publish Individual assignment without recipients.");

        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unpublish assignment
    /// </summary>
    public void Unpublish()
    {
        if (!IsPublished)
            throw new InvalidOperationException("Assignment is not published.");

        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }
}