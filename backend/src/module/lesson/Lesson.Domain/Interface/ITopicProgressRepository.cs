namespace Lesson.Domain.Interface;

public interface ITopicProgressRepository
{
    Task<TopicProgressAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TopicProgressAggregate?> GetByUserIdAndTopicIdAsync(Guid userId, Guid topicId, CancellationToken ct = default);
    Task AddAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default);
    Task UpdateAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}