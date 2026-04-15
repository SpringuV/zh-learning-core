namespace Lesson.Infrastructure.Repository;

public class TopicRepository(LessonDbContext dbContext, ILogger<TopicRepository> logger) : LessonRepositoryBase(logger), ITopicRepository
{
    private readonly LessonDbContext _dbContext = dbContext;

    public async Task AddAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.Topics.AddAsync(topic, ct);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when adding topic: {TopicId}",
            "Unexpected error adding topic",
            "Không thể thêm topic vào database",
            topic.TopicId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                var topic = await GetByIdAsync(id, ct);
                if (topic != null)
                {
                    _dbContext.Topics.Remove(topic);
                    // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
                }
            },
            "Database error when deleting topic: {TopicId}",
            "Unexpected error deleting topic",
            "Không thể xóa topic khỏi database",
            id);
    }

    public async Task<IEnumerable<TopicAggregate>> GetAllByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                // Sử dụng Where để lọc các topic có TopicId nằm trong danh sách ids,
                // sau đó chuyển kết quả thành List, nó sẽ chạy một truy vấn SQL với điều kiện IN
                // để lấy tất cả các topic có ID khớp trong một lần truy vấn.
                return await _dbContext.Topics.Where(t => ids.Contains(t.TopicId)).ToListAsync(ct);
            },
            "Database error getting topics by IDs",
            "Unexpected error getting topics by IDs",
            "Không thể lấy các topic theo IDs");
    }

    public async Task<IEnumerable<TopicAggregate>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
    {
        return await _dbContext.Topics.Where(t => t.CourseId == courseId).ToListAsync(ct);
    }

    public async Task<TopicAggregate?> GetByIdAsync(Guid TopicId, CancellationToken ct = default)
    {
        return await _dbContext.Topics.FindAsync([TopicId], ct); // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của TopicAggregate.
    }

    public async Task<IEnumerable<TopicAggregate>> GetByIdsAndCourseIdAsync(Guid courseId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                return await _dbContext.Topics.Where(t => t.CourseId == courseId && ids.Contains(t.TopicId)).ToListAsync(ct);
            },
            "Database error getting topics by course ID and topic IDs",
            "Unexpected error getting topics by course ID and topic IDs",
            "Không thể lấy các topic theo course ID và topic IDs",
            courseId, string.Join(",", ids)
        );
    }

    public async Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                // Keep projection nullable so empty Topics table returns null instead of throwing.
                return await _dbContext.Topics
                    .Select(t => (int?)t.OrderIndex)
                    .MaxAsync(ct);
            },
            "Database error getting max OrderIndex",
            "Error getting max OrderIndex",
            "Không thể lấy OrderIndex tối đa");
    }

    public async Task UpdateAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        await ExecuteAsync(
            () =>
            {
                _dbContext.Topics.Update(topic);
                // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            },
            "Database error when updating topic: {TopicId}",
            "Unexpected error updating topic",
            "Không thể cập nhật topic",
            topic.TopicId);
    }

    public Task UpdateRangeAsync(IEnumerable<TopicAggregate> topics, CancellationToken ct = default)
    {
        return ExecuteAsync(() =>
            {
                _dbContext.Topics.UpdateRange(topics);
                // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            },
            "Database error when updating topics",
            "Unexpected error updating topics",
            "Không thể cập nhật các topic");
    }
}