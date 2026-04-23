using Microsoft.Extensions.Caching.Distributed;

namespace Lesson.Infrastructure.Repository;

public class TopicProgressRepository(IDistributedCache cache, LessonDbContext dbContext, ILogger<TopicProgressRepository> logger) : LessonRepositoryBase(logger), ITopicProgressRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly IDistributedCache _cache = cache;

    // add cache
    public async Task<TopicProgressAggregate?> GetByUserIdAndTopicIdAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.TopicProgresses
                .FirstOrDefaultAsync(tp => tp.UserId == userId && tp.TopicId == topicId, cancellationToken),
            "Database error when retrieving topic progress by user/topic: {UserId}, {TopicId}",
            "Unexpected error retrieving topic progress by user/topic",
            "Không thể truy xuất topic progress theo user/topic",
            userId, topicId);
    }

    public async Task AddAsync(TopicProgressAggregate topicProgress, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.TopicProgresses.AddAsync(topicProgress, cancellationToken);
                // UnitOfWork sẽ gọi SaveChangesAsync.
            },
            "Database error when adding topic progress: {TopicProgressId}",
            "Unexpected error adding topic progress",
            "Không thể thêm topic progress vào database",
            topicProgress.TopicProgressId);
    }

    public async Task<TopicProgressAggregate?> GetByIdAsync(Guid topicProgressId, CancellationToken cancellationToken = default)
    {
        // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của TopicProgressAggregate.
        return await _dbContext.TopicProgresses.FindAsync([topicProgressId], cancellationToken);
    }

    public async Task UpdateAsync(TopicProgressAggregate topicProgress, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                _dbContext.TopicProgresses.Update(topicProgress);
                // UnitOfWork sẽ gọi SaveChangesAsync.
            },
            "Database error when updating topic progress: {TopicProgressId}",
            "Unexpected error updating topic progress",
            "Không thể cập nhật topic progress",
            topicProgress.TopicProgressId);
    }

    public async Task DeleteAsync(Guid topicProgressId, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                var topicProgress = await GetByIdAsync(topicProgressId, cancellationToken);
                if (topicProgress != null)
                {
                    _dbContext.TopicProgresses.Remove(topicProgress);
                    // UnitOfWork sẽ gọi SaveChangesAsync.
                }
            },
            "Database error when deleting topic progress: {TopicProgressId}",
            "Unexpected error deleting topic progress",
            "Không thể xóa topic progress khỏi database",
            topicProgressId);
    }
}