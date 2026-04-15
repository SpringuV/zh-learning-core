using Lesson.Domain.Entities;

namespace Lesson.Domain.Interface;

public interface ITopicRepository
{
    Task<TopicAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TopicAggregate>> GetByIdsAndCourseIdAsync(Guid courseId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IEnumerable<TopicAggregate>> GetAllByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<TopicAggregate> topics, CancellationToken ct = default);
    Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default);
    Task<IEnumerable<TopicAggregate>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);
    Task AddAsync(TopicAggregate topic, CancellationToken ct = default);
    Task UpdateAsync(TopicAggregate topic, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}