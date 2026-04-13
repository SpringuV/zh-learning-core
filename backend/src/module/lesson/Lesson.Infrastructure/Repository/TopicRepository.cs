namespace Lesson.Infrastructure.Repository;

public class TopicRepository(LessonDbContext dbContext, ILogger<TopicRepository> logger, IPublisher publisher) : ITopicRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<TopicRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public async Task AddAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        try
        {
            await _dbContext.Topics.AddAsync(topic, ct);
            // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
        } catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when adding topic: {TopicId}", topic.TopicId);
            throw new RepositoryException("Không thể thêm topic vào database", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding topic");
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var topic = await GetByIdAsync(id, ct);
            if (topic != null)
            {
                _dbContext.Topics.Remove(topic);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when deleting topic: {TopicId}", id);
            throw new RepositoryException("Không thể xóa topic khỏi database", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting topic");
            throw;
        }
    }

    public async Task<IEnumerable<TopicAggregate>> GetAllByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        try
        {
            // Sử dụng Where để lọc các topic có TopicId nằm trong danh sách ids, 
            // sau đó chuyển kết quả thành List, nó sẽ chạy một truy vấn SQL với điều kiện IN 
            // để lấy tất cả các topic có ID khớp trong một lần truy vấn.
            return await _dbContext.Topics.Where(t => ids.Contains(t.TopicId)).ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting topics by IDs");
            throw new RepositoryException("Không thể lấy các topic theo IDs", ex);
        }
    }

    public async Task<IEnumerable<TopicAggregate>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
    {
        return await _dbContext.Topics.Where(t => t.CourseId == courseId).ToListAsync(ct);
    }

    public async Task<TopicAggregate?> GetByIdAsync(Guid TopicId, CancellationToken ct = default)
    {
        return await _dbContext.Topics.FindAsync([TopicId], ct); // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của TopicAggregate.
    }

    public async Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default)
    {
        try
        {
            // Keep projection nullable so empty Topics table returns null instead of throwing.
            return await _dbContext.Topics
                .Select(t => (int?)t.OrderIndex)
                .MaxAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting max OrderIndex");
            throw new RepositoryException("Không thể lấy OrderIndex tối đa", ex);
        }
    }
    

    public Task SaveAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        try
        {
            _dbContext.Topics.Update(topic);
            // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when updating topic: {TopicId}", topic.TopicId);
            throw new RepositoryException("Không thể cập nhật topic", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating topic");
            throw;
        }
    }

    public Task UpdateRangeAsync(IEnumerable<TopicAggregate> topics, CancellationToken ct = default)
    {
        try
        {
            _dbContext.Topics.UpdateRange(topics);
            // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            return Task.CompletedTask;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when updating topics");
            throw new RepositoryException("Không thể cập nhật các topic", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating topics");
            throw;
        }
    }
}