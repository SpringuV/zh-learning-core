    namespace Classroom.Infrastructure.Repository;

public class ClassroomStudentRepository(ClassroomDbContext context) : IClassroomStudentRepository
{
    private readonly ClassroomDbContext _context = context;
    
    public Task AddAsync(ClassroomStudentAggregate student, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ClassroomStudentAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClassroomStudentAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ClassroomStudentAggregate student, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ClassroomStudentAggregate student, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}