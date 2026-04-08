using Ardalis.Specification.EntityFrameworkCore;
using Classroom.Domain.Entities;
using Classroom.Domain.Entities.Assignment;
using Classroom.Domain.Interface;
using Classroom.Infrastructure.Specification;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Infrastructure.Repository;

public class ClassroomEnrollmentRepository(ClassroomDbContext context) : IClassroomEnrollmentRepository
{
    private readonly ClassroomDbContext _context = context;
    
    public Task AddAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ClassroomEnrollmentAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClassroomEnrollmentAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ClassroomEnrollmentAggregate enrollment, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}