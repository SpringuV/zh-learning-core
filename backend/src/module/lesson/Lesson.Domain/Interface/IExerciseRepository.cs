using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;

namespace Lesson.Domain.Interface;

public interface IExerciseRepository
{
    Task<ExerciseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task<IEnumerable<ExerciseAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task UpdateAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}