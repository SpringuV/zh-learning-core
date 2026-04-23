namespace Lesson.Domain.Interface;

public interface IExerciseRepository
{
    Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAsync(Guid topicId, CancellationToken ct = default);
    Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAndIdsAsync(Guid topicId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task ReorderByIdsAndTopicIdAsync(Guid topicId, IReadOnlyList<Guid> orderedExerciseIds, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<ExerciseAggregate> exercises, CancellationToken ct = default);
    Task<int?> GetMaxOrderIndexByTopicIdAsync(Guid topicId, CancellationToken ct = default);
    Task<ExerciseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task UpdateAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}