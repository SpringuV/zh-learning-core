using Lesson.Domain.Entities;

namespace Lesson.Domain.Interface;

public interface ITopicRepository
{
    Task<TopicAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(TopicAggregate topic, CancellationToken ct = default);
    Task<IEnumerable<TopicAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TopicAggregate topic, CancellationToken ct = default);
    Task UpdateAsync(TopicAggregate topic, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}