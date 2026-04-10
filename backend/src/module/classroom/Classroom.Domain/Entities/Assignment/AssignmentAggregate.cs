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

public enum DurationTimeMinutes 
{
    None = 0,
    Minutes20 = 20,
    Minutes40 = 40,
    Minutes60 = 60,
    Minutes80 = 80,
    Minutes100 = 100,
    Minutes120 = 120
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
    public DurationTimeMinutes? DurationMinutes { get; private set; }      // Thời gian tối đa làm bài (phút). Null = không giới hạn
    public bool IsTimedAssignment { get; private set; } = false;  // Bài tập có giới hạn thời gian hay không
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
        DateTime dueDate,
        DurationTimeMinutes? durationMinutes = null)
    {
        // Validation
        if (classroomId == Guid.Empty)
            throw new ArgumentException("ClassroomId không được để trống.", nameof(classroomId));
        if (teacherId == Guid.Empty)
            throw new ArgumentException("TeacherId không được để trống.", nameof(teacherId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống.", nameof(title));
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Ngày hết hạn phải lớn hơn ngày hiện tại.", nameof(dueDate));
        if (durationMinutes.HasValue && durationMinutes <= DurationTimeMinutes.None)
            throw new ArgumentException("Thời gian làm bài phải là số dương.", nameof(durationMinutes));

        var assignment = new AssignmentAggregate
        {
            AssignmentId = Guid.CreateVersion7(),
            ClassroomId = classroomId,
            TeacherId = teacherId,
            Title = title,
            Description = description,
            DurationMinutes = durationMinutes,
            IsTimedAssignment = durationMinutes.HasValue,   // Auto-set based on duration
            AssignmentType = assignmentType,
            SkillFocus = skillFocus,
            DueDate = dueDate,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        assignment.AddDomainEvent(new AssignmentCreatedEvent(
            assignment.AssignmentId,
            assignment.ClassroomId,
            assignment.TeacherId,
            assignment.Title,
            assignment.Description,
            assignment.AssignmentType.ToString(),
            assignment.SkillFocus.ToString(),
            assignment.DueDate,
            assignment.IsTimedAssignment,
            assignment.DurationMinutes,
            assignment.IsPublished,
            assignment.Exercises.Count,
            assignment.Recipients.Count,
            assignment.CreatedAt,
            assignment.UpdatedAt
        ));
        return assignment;
    }

    public void UpdateTitle(string newTitle)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty.", nameof(newTitle));
        
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentTitleUpdatedEvent(AssignmentId, newTitle, UpdatedAt));
    }

    public void UpdateDescription(string newDescription)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        
        Description = newDescription ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentDescriptionUpdatedEvent(AssignmentId, newDescription, UpdatedAt));
    }

    public void UpdateDueDate(DateTime newDueDate)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        if (newDueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be greater than current time.", nameof(newDueDate));
        
        DueDate = newDueDate;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentDueDateUpdatedEvent(AssignmentId, newDueDate, UpdatedAt));
    }

    public void UpdateSkillFocus(SkillFocus newSkillFocus)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        
        SkillFocus = newSkillFocus;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentSkillFocusUpdatedEvent(AssignmentId, newSkillFocus.ToString(), UpdatedAt));
    }

    public void UpdateAssignmentType(AssignmentType newAssignmentType)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        if (newAssignmentType == AssignmentType.Individual && _recipients.Count == 0)
            throw new InvalidOperationException("Cannot change to Individual type without recipients.");
        
        AssignmentType = newAssignmentType;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentTypeUpdatedEvent(AssignmentId, newAssignmentType.ToString(), UpdatedAt));
    }

    public void UpdateDurationMinutes(DurationTimeMinutes? newDurationMinutes)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot update details of a published assignment.");
        if (newDurationMinutes.HasValue && newDurationMinutes <= DurationTimeMinutes.None)
            throw new ArgumentException("Duration must be positive.", nameof(newDurationMinutes));
        
        DurationMinutes = newDurationMinutes;
        IsTimedAssignment = newDurationMinutes.HasValue;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentDurationUpdatedEvent(AssignmentId, newDurationMinutes?.ToString(), IsTimedAssignment, UpdatedAt));
    }

    /// <summary>
    /// Add exercise to assignment
    /// </summary>
    public void AddAssignmentExercise(Guid exerciseId, int orderIndex)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot add exercise to published assignment.");

        var exercise = AssignmentExercise.Create(AssignmentId, exerciseId, orderIndex);
        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentExerciseAddedEvent(
            AssignmentId,
            exerciseId,
            orderIndex,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Remove exercise from assignment
    /// </summary>
    public void RemoveAssignmentExercise(Guid exerciseId)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot remove exercise from published assignment.");

        var exercise = _exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
        if (exercise != null)
        {
            _exercises.Remove(exercise);
            UpdatedAt = DateTime.UtcNow;
        }
        AddDomainEvent(new AssignmentExerciseRemovedEvent(
            AssignmentId,
            exerciseId,
            _exercises.Count,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Add recipient for Individual assignment type
    /// </summary>
    public void AddAssignmentRecipient(Guid studentId)
    {
        if (AssignmentType != AssignmentType.Individual)
            throw new InvalidOperationException("Recipients can only be added to Individual assignment type.");

        var recipient = AssignmentRecipient.Create(AssignmentId, studentId);
        _recipients.Add(recipient);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AssignmentRecipientAddedEvent(
            AssignmentId,
            studentId,
            _recipients.Count,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Remove recipient from Individual assignment
    /// </summary>
    public void RemoveAssignmentRecipient(Guid studentId)
    {
        var recipient = _recipients.FirstOrDefault(r => r.StudentId == studentId);
        if (recipient != null)
        {
            _recipients.Remove(recipient);
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new AssignmentRecipientRemovedEvent(
                AssignmentId,
                studentId,
                _recipients.Count,
                UpdatedAt
            ));
        }
    }
    
    /// <summary>
    /// Add multiple recipients in bulk (BULK OPERATION)
    /// </summary>
    public void AddAssignmentRecipientBulk(List<Guid> studentIds)
    {
        if (AssignmentType != AssignmentType.Individual)
            throw new InvalidOperationException("Phiếu giao bài tập chỉ có thể thêm người nhận cho loại bài tập Individual.");
        if (studentIds == null || studentIds.Count == 0)
            throw new ArgumentException("Danh sách học sinh không được để trống.", nameof(studentIds));

        var addedCount = 0;
        foreach (var studentId in studentIds)
        {
            if (studentId == Guid.Empty) continue;
            if (!_recipients.Any(r => r.StudentId == studentId))
            {
                var recipient = AssignmentRecipient.Create(AssignmentId, studentId);
                _recipients.Add(recipient);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            UpdatedAt = DateTime.UtcNow;
            // Emit bulk recipient added event
            AddDomainEvent(new AssignmentRecipientsAddedBulkEvent(
                AssignmentId,
                ClassroomId,
                studentIds.Where(id => id != Guid.Empty).ToList(),
                _recipients.Count,
                UpdatedAt
            ));
        }
    }
    
    /// <summary>
    /// Remove multiple recipients in bulk (BULK OPERATION)
    /// </summary>
    public void RemoveAssignmentRecipientBulk(List<Guid> studentIds)
    {
        if (studentIds == null || studentIds.Count == 0)
            throw new ArgumentException("Danh sách học sinh không được để trống.", nameof(studentIds));

        var removedCount = 0;
        foreach (var studentId in studentIds)
        {
            var recipient = _recipients.FirstOrDefault(r => r.StudentId == studentId);
            if (recipient != null)
            {
                _recipients.Remove(recipient);
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            UpdatedAt = DateTime.UtcNow;
            // Emit bulk recipient removed event
            AddDomainEvent(new AssignmentRecipientsRemovedBulkEvent(
                AssignmentId,
                ClassroomId,
                studentIds.Where(id => id != Guid.Empty).ToList(),
                _recipients.Count,
                UpdatedAt
            ));
        }
    }

    /// <summary>
    /// Publish assignment (make visible to students)
    /// </summary>
    public void PublishAssignment()
    {
        if (IsPublished)
            throw new InvalidOperationException("Assignment is already published.");
        if (_exercises.Count == 0)
            throw new InvalidOperationException("Cannot publish assignment without exercises.");
        if (AssignmentType == AssignmentType.Individual && _recipients.Count == 0)
            throw new InvalidOperationException("Cannot publish Individual assignment without recipients.");

        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentPublishedEvent(
            AssignmentId,
            ClassroomId,
            IsPublished,
            UpdatedAt
        ));
    }

    /// <summary>
    /// Unpublish assignment
    /// </summary>
    public void UnpublishAssignment()
    {
        if (!IsPublished)
            throw new InvalidOperationException("Assignment is not published.");

        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AssignmentUnpublishedEvent(
            AssignmentId,
            ClassroomId,
            IsPublished,
            UpdatedAt
        ));
    }
}