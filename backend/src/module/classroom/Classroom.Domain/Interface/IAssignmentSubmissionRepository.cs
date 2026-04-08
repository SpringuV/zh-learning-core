using Classroom.Domain.Entities.Assignment;

namespace Classroom.Domain.Interface;

public interface IAssignmentSubmissionRepository
{
    Task<AssignmentSubmissionAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default);
    Task<IEnumerable<AssignmentSubmissionAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default);
    Task UpdateAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}