namespace Classroom.Domain.Interface;

public interface IClassroomStudentRepository
{
    Task<ClassroomStudentAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(ClassroomStudentAggregate classroomStudent, CancellationToken ct = default);
    Task<IEnumerable<ClassroomStudentAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ClassroomStudentAggregate classroomStudent, CancellationToken ct = default);
    Task UpdateAsync(ClassroomStudentAggregate classroomStudent, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}