namespace Classroom.Domain.Interface;

public interface IClassroomEnrollmentRepository
{
    Task<ClassroomEnrollmentAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default);
    Task<IEnumerable<ClassroomEnrollmentAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default);
    Task UpdateAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}