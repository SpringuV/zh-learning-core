
using Classroom.Domain.Entities;

namespace Classroom.Domain.Interface;

public interface IClassroomRepository
{
    Task<ClassroomAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(ClassroomAggregate classroom, CancellationToken ct = default);
    Task<IEnumerable<ClassroomAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ClassroomAggregate classroom, CancellationToken ct = default);
    Task UpdateAsync(ClassroomAggregate classroom, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}