namespace Lesson.Infrastructure.Repository;

public class ExerciseRepository(LessonDbContext dbContext, ILogger<ExerciseRepository> logger, IPublisher publisher) : IExerciseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<ExerciseRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public Task AddAsync(ExerciseAggregate exercise, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ExerciseAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExerciseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ExerciseAggregate exercise, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ExerciseAggregate exercise, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}