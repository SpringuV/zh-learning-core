namespace Classroom.Domain.Entities;

public enum Currency
{
    VND,
    USD,
}

public enum Status
{
    Upcoming,
    RegistrationOpen,
    RegistrationClosed,
    Ongoing,
    Completed,
    Cancelled
}
public class ClassroomAggregate : BaseAggregateRoot
{
    public Guid ClassroomId { get; private set; }
    public Guid TeacherId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty; // URL-friendly identifier for the classroom
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public int HskLevel { get; private set; } 
    public DateTime? StartDate { get; private set; } 
    public DateTime? EndDate { get; private set; } 
    public string ScheduleInfo { get; private set; } = string.Empty; // Thông tin lịch học (ví dụ: "Thứ 2,4,6 - 18:00-19:30")
    public float Price { get; private set; } 
    public Currency PriceCurrency { get; private set; } = Currency.VND; 
    public Status ClassroomStatus { get; private set; } = Status.Upcoming;

    private readonly List<Guid> _StudentIds = [];
    public IReadOnlyList<Guid> StudentIds => _StudentIds.AsReadOnly();
    
    public static ClassroomAggregate Create(string title, string description, Guid teacherId, int hskLevel, DateTime? startDate = null, DateTime? endDate = null, string scheduleInfo = "", float price = 0, Currency priceCurrency = Currency.VND)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tiêu đề lớp học không được để trống.", nameof(title));
        if (teacherId == Guid.Empty) throw new ArgumentException("TeacherId không được để trống.", nameof(teacherId));
        if (hskLevel < 1 || hskLevel > 6) throw new ArgumentException("Cấp độ HSK phải là một số từ 1 đến 6.", nameof(hskLevel));

        var classroom = new ClassroomAggregate
        {
            ClassroomId = Guid.CreateVersion7(),
            Title = title,
            Description = description,
            TeacherId = teacherId,
            HskLevel = hskLevel,
            StartDate = startDate,
            EndDate = endDate,
            ScheduleInfo = scheduleInfo,
            Price = price,
            PriceCurrency = priceCurrency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Slug = GenerateSlug(title),
            ClassroomStatus = Status.Upcoming
        };

        // Emit domain event for new classroom creation
        classroom.AddDomainEvent(new ClassroomCreatedEvent(
            classroom.ClassroomId,
            classroom.Title,
            classroom.Description,
            classroom.TeacherId,
            classroom.HskLevel,
            classroom.StartDate,
            classroom.EndDate,
            classroom.ScheduleInfo,
            classroom.Price,
            classroom.PriceCurrency,
            classroom.CreatedAt,    
            classroom.UpdatedAt,
            classroom.Slug,
            classroom.ClassroomStatus.ToString()
        ));        
        return classroom;
    }

    public void ChangeStatus(Status newStatus)
    {
        if (ClassroomStatus == newStatus) return; // No change
        
        ClassroomStatus = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ClassroomStatusChangedEvent(
            ClassroomId,
            newStatus.ToString(),
            UpdatedAt
        ));
    }

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Tiêu đề lớp học không được để trống.", nameof(newTitle));
        
        Title = newTitle;
        Slug = GenerateSlug(newTitle);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomTitleUpdatedEvent(ClassroomId, newTitle, Slug, UpdatedAt));
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Mô tả lớp học không được để trống.", nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomDescriptionUpdatedEvent(ClassroomId, newDescription, UpdatedAt));
    }

    public void UpdateHskLevel(int newHskLevel)
    {
        if (newHskLevel < 1 || newHskLevel > 6)
            throw new ArgumentException("Cấp độ HSK phải là một số từ 1 đến 6.", nameof(newHskLevel));
        
        HskLevel = newHskLevel;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomHskLevelUpdatedEvent(ClassroomId, newHskLevel, UpdatedAt));
    }

    public void UpdateStartDate(DateTime newStartDate)
    {
        StartDate = newStartDate;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomStartDateUpdatedEvent(ClassroomId, newStartDate, UpdatedAt));
    }

    public void UpdateEndDate(DateTime newEndDate)
    {
        EndDate = newEndDate;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomEndDateUpdatedEvent(ClassroomId, newEndDate, UpdatedAt));
    }

    public void UpdateScheduleInfo(string newScheduleInfo)
    {
        if (string.IsNullOrWhiteSpace(newScheduleInfo))
            throw new ArgumentException("Thông tin lịch học không được để trống.", nameof(newScheduleInfo));
        ScheduleInfo = newScheduleInfo;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomScheduleInfoUpdatedEvent(ClassroomId, newScheduleInfo, UpdatedAt));
    }

    public void UpdatePrice(float newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Giá không được âm.", nameof(newPrice));
        
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomPriceUpdatedEvent(ClassroomId, newPrice, UpdatedAt));
    }

    public void UpdatePriceCurrency(Currency newPriceCurrency)
    {
        PriceCurrency = newPriceCurrency;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ClassroomPriceCurrencyUpdatedEvent(ClassroomId, newPriceCurrency.ToString(), UpdatedAt));
    }
    
    public void AddStudent(Guid studentId)
    {
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId không được để trống.", nameof(studentId));
        if (_StudentIds.Contains(studentId)) throw new InvalidOperationException("Học sinh đã có trong lớp.");
        
        _StudentIds.Add(studentId);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ClassroomStudentIndividualAddedEvent(
            ClassroomId,
            studentId,
            _StudentIds.Count,
            UpdatedAt
        ));
    }
    
    public void RemoveStudent(Guid studentId)
    {
        if (!_StudentIds.Contains(studentId)) throw new InvalidOperationException("Học sinh không tồn tại trong lớp.");
        
        _StudentIds.Remove(studentId);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ClassroomStudentIndividualRemovedEvent(
            ClassroomId,
            studentId,
            _StudentIds.Count,
            UpdatedAt
        ));
    }
    
    public void AddStudentBulk(List<Guid> studentIds)
    {
        if (studentIds == null || studentIds.Count == 0) 
            throw new ArgumentException("Danh sách học sinh không được để trống.", nameof(studentIds));
        
        foreach (var studentId in studentIds)
        {
            if (studentId == Guid.Empty) continue;
            if (!_StudentIds.Contains(studentId))
            {
                _StudentIds.Add(studentId);
            }
        }
        
        UpdatedAt = DateTime.UtcNow;
        
        // Emit bulk enrollment event
        AddDomainEvent(new ClassroomStudentsEnrolledBulkEvent(
            ClassroomId,
            [.. studentIds.Where(id => id != Guid.Empty)],
            _StudentIds.Count,
            UpdatedAt
        ));
    }
    
    public void RemoveStudentBulk(List<Guid> studentIds)
    {
        if (studentIds == null || studentIds.Count == 0) 
            throw new ArgumentException("Danh sách học sinh không được để trống.", nameof(studentIds));
        
        foreach (var studentId in studentIds)
        {
            if (_StudentIds.Contains(studentId))
            {
                _StudentIds.Remove(studentId);
            }
        }
        
        UpdatedAt = DateTime.UtcNow;
        // Emit bulk removal event
        AddDomainEvent(new ClassroomStudentsRemovedBulkEvent(
            ClassroomId,
            [.. studentIds.Where(id => id != Guid.Empty)],
            _StudentIds.Count,
            UpdatedAt
        ));
    }
}