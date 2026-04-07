using Lesson.Domain.Entities.Exercise;

namespace Lesson.Domain.Interface;

public interface IExerciseAttemptRepository
{
    Task<ExerciseAttemptAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default);
    Task<IEnumerable<ExerciseAttemptAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default);
    Task UpdateAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}