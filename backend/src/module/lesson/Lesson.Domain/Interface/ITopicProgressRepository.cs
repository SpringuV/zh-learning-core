using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;

namespace Lesson.Domain.Interface;

public interface ITopicProgressRepository
{
    Task<TopicProgressAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default);
    Task<IEnumerable<TopicProgressAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default);
    Task UpdateAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}