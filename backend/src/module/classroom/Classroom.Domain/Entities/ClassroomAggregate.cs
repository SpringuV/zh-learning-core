using HanziAnhVu.Shared.Domain;

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
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public int HskLevel { get; private set; } // Cấp độ HSK của lớp học (1-6)
    public DateTime? StartDate { get; private set; } // Ngày bắt đầu lớp
    public DateTime? EndDate { get; private set; } // Ngày kết thúc lớp (nếu có)
    public string ScheduleInfo { get; private set; } = string.Empty; // Thông tin lịch học (ví dụ: "Thứ 2,4,6 - 18:00-19:30")
    public float Price { get; private set; } // Giá tiền của lớp học
    public Currency PriceCurrency { get; private set; } = Currency.VND; // Đơn vị tiền tệ của giá tiền
    public Status ClassroomStatus { get; private set; } = Status.Upcoming; // Trạng thái của lớp học

    private readonly List<Guid> _StudentIds = [];
    public IReadOnlyList<Guid> StudentIds => _StudentIds.AsReadOnly();
    
    public static ClassroomAggregate Create(string title, string description, Guid teacherId, int hskLevel)
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return classroom;
    }
    
    public void AddStudent(Guid studentId)
    {
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId không được để trống.", nameof(studentId));
        if (_StudentIds.Contains(studentId)) throw new InvalidOperationException("Học sinh đã có trong lớp.");
        
        _StudentIds.Add(studentId);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveStudent(Guid studentId)
    {
        if (!_StudentIds.Contains(studentId)) throw new InvalidOperationException("Học sinh không tồn tại trong lớp.");
        
        _StudentIds.Remove(studentId);
        UpdatedAt = DateTime.UtcNow;
    }
}