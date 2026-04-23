namespace Lesson.Infrastructure.Repository;

public class FlashcardRepository(LessonDbContext dbContext, ILogger<FlashcardRepository> logger) 
    : LessonRepositoryBase(logger), IFlashcardRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    public async Task AddAsync(FlashcardAggregate flashcard, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.Flashcards.AddAsync(flashcard, ct);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when adding flashcard: {FlashcardId}",
            "Unexpected error adding flashcard",
            "Không thể thêm flashcard vào database",
            flashcard.FlashcardId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var flashcard = await GetByIdAsync(id, ct);
        if (flashcard == null)
        {
            Logger.LogWarning("Flashcard with ID {FlashcardId} not found for deletion", id);
            return;
        }

        await ExecuteAsync(
            () =>
            {
                _dbContext.Flashcards.Remove(flashcard);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when deleting flashcard: {FlashcardId}",
            "Unexpected error deleting flashcard",
            "Không thể xóa flashcard khỏi database",
            id);
    }

    public async Task<FlashcardAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Flashcards.FindAsync([id], ct),
            "Database error when retrieving flashcard: {FlashcardId}",
            "Unexpected error retrieving flashcard",
            "Không thể truy xuất flashcard từ database",
            id);
    }

    public async Task<IEnumerable<FlashcardAggregate>> GetByTopicIdAndIdsAsync(Guid topicId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Flashcards
                .Where(f => f.TopicId == topicId && ids.Contains(f.FlashcardId))
                .ToListAsync(ct),
            "Database error retrieving flashcards by topic ID and flashcard IDs: {TopicId}, {FlashcardIds}",
            "Unexpected error retrieving flashcards by topic ID and flashcard IDs: {TopicId}, {FlashcardIds}",
            "Không thể truy xuất flashcards theo topic ID và flashcard IDs",
            topicId, string.Join(",", ids)
        );
    }

    public async Task<IEnumerable<FlashcardAggregate>> GetByTopicIdAsync(Guid topicId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Flashcards.Where(f => f.TopicId == topicId).ToListAsync(ct),
            "Database error retrieving flashcards by topic ID: {TopicId}",
            "Unexpected error retrieving flashcards by topic ID: {TopicId}",
            "Không thể truy xuất flashcards theo topic ID",
            topicId);
    }

    public async Task UpdateAsync(FlashcardAggregate flashcard, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                _dbContext.Flashcards.Update(flashcard);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when updating flashcard: {FlashcardId}",
            "Unexpected error updating flashcard",
            "Không thể cập nhật flashcard vào database",
            flashcard.FlashcardId);
    }
}