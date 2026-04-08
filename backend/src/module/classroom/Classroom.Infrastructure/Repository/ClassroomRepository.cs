using Ardalis.Specification.EntityFrameworkCore;
using Classroom.Domain.Entities;
using Classroom.Domain.Entities.Assignment;
using Classroom.Domain.Interface;
using Classroom.Infrastructure.Specification;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Infrastructure.Repository;

public class ClassroomRepository(ClassroomDbContext context) : IClassroomRepository
{
    private readonly ClassroomDbContext _context = context;
    
    public Task AddAsync(ClassroomAggregate classroom, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ClassroomAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClassroomAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ClassroomAggregate classroom, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ClassroomAggregate classroom, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}