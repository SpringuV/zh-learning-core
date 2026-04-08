using Ardalis.Specification.EntityFrameworkCore;
using Classroom.Domain.Entities.Assignment;
using Classroom.Domain.Interface;
using Classroom.Infrastructure.Specification;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Infrastructure.Repository;

public class AssignmentSubmissionRepository(ClassroomDbContext context) : IAssignmentSubmissionRepository
{
    private readonly ClassroomDbContext _context = context;
    
    public Task AddAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AssignmentSubmissionAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<AssignmentSubmissionAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(AssignmentSubmissionAggregate submission, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}