using HanziAnhVu.Shared.Domain;

namespace Classroom.Domain.Entities;

public enum StatusClass
{
    Active,
    Removed
}
// teacher confirm student enrollment, then add to classroom student list
public class ClassroomStudentAggregate : BaseAggregateRoot
{
    public Guid ClassroomStudentId { get; private set; }
    public Guid ClassroomId { get; private set; }
    public Guid StudentId { get; private set; } // Liên kết đến UserId của học viên, soft link
    public Guid AddedBy { get; private set; } // UserId của người thêm (thường là teacherId), soft link
    public DateTime JoinedAt { get; private set; }
    public StatusClass Status { get; private set; } = StatusClass.Active;
    public DateTime UpdatedAt { get; private set; }

    public static ClassroomStudentAggregate Create(Guid classroomId, Guid studentId, Guid addedById)
    {
        if (classroomId == Guid.Empty) throw new ArgumentException("ClassroomId không được để trống.", nameof(classroomId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId không được để trống.", nameof(studentId));
        if (addedById == Guid.Empty) throw new ArgumentException("AddedById không được để trống.", nameof(addedById));

        var classroomStudent = new ClassroomStudentAggregate
        {
            ClassroomStudentId = Guid.CreateVersion7(),
            ClassroomId = classroomId,
            StudentId = studentId,
            AddedBy = addedById,
            JoinedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return classroomStudent;
    }
    
}