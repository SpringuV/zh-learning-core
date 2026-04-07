using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;
using Lesson.Domain.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lesson.Infrastructure.Repository;

public class ExerciseAttemptRepository(LessonDbContext dbContext, ILogger<ExerciseAttemptRepository> logger, IPublisher publisher) : IExerciseAttemptRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<ExerciseAttemptRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public Task AddAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ExerciseAttemptAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExerciseAttemptAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}