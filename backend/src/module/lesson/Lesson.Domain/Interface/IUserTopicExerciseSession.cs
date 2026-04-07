using Lesson.Domain.Entities.Exercise;

namespace Lesson.Domain.Interface;

public interface IUserTopicExerciseSessionRepository
{
    Task<UserTopicExerciseSessionAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(UserTopicExerciseSessionAggregate userTopicExerciseSession, CancellationToken ct = default);
    Task<IEnumerable<UserTopicExerciseSessionAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UserTopicExerciseSessionAggregate userTopicExerciseSession, CancellationToken ct = default);
    Task UpdateAsync(UserTopicExerciseSessionAggregate userTopicExerciseSession, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}