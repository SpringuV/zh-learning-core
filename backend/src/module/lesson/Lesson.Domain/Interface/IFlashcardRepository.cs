using Lesson.Domain.Entities;

namespace Lesson.Domain.Interface;

public interface IFlashcardRepository
{
    Task<IEnumerable<FlashcardAggregate>> GetByTopicIdAsync(Guid topicId, CancellationToken ct = default);
    Task<IEnumerable<FlashcardAggregate>> GetByTopicIdAndIdsAsync(Guid topicId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<FlashcardAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(FlashcardAggregate flashcard, CancellationToken ct = default);
    Task UpdateAsync(FlashcardAggregate flashcard, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

}