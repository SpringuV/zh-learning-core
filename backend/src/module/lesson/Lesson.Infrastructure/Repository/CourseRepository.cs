namespace Lesson.Infrastructure.Repository;

// nơi save sẽ là unit of work
public class CourseRepository(LessonDbContext dbContext, ILogger<CourseRepository> logger) : ICourseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<CourseRepository> _logger = logger;

    public Task<IEnumerable<CourseAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        try
        {   
            _logger.LogInformation("[CourseRepository.AddAsync] Adding course {CourseId} to DbSet - Title: {Title}", course.CourseId, course.Title);
            // Chỉ thêm vào DbSet, không save - để UnitOfWork xử lý
            await _dbContext.Courses.AddAsync(course, cancellationToken);
            _logger.LogInformation("[CourseRepository.AddAsync] Course {CourseId} added to DbSet successfully", course.CourseId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when adding course: {CourseId}", course.CourseId);
            throw new RepositoryException("Không thể thêm khóa học vào database", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding course");
            throw;
        }
    }

    public async Task<CourseAggregate?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của CourseAggregate.
        return await _dbContext.Courses.FindAsync([courseId], cancellationToken);
    }

    public async Task UpdateAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.Courses.Update(course);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when updating course: {CourseId}", course.CourseId);
            throw new RepositoryException("Không thể cập nhật khóa học", ex);
        }
    }

    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var course = await GetByIdAsync(courseId, cancellationToken);
            if (course != null)
            {
                _dbContext.Courses.Remove(course);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when deleting course: {CourseId}", courseId);
            throw new RepositoryException("Không thể xóa khóa học", ex);
        }
    }

    public async Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default)
    {
        try
        {
            return await _dbContext.Courses.MaxAsync(c => (int?)c.OrderIndex ?? 0, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting max OrderIndex");
            throw new RepositoryException("Không thể lấy OrderIndex tối đa", ex);
        }
    }

    public async Task<IEnumerable<CourseAggregate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        try
        {
            // Sử dụng Where để lọc các khóa học có CourseId nằm trong danh sách ids, 
            // sau đó chuyển kết quả thành List, nó sẽ chạy một truy vấn SQL với điều kiện IN 
            // để lấy tất cả các khóa học có ID khớp trong một lần truy vấn.
            return await _dbContext.Courses.Where(c => ids.Contains(c.CourseId)).ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses by IDs");
            throw new RepositoryException("Không thể lấy khóa học theo IDs", ex);
        }
    }

    public Task UpdateRangeAsync(IEnumerable<CourseAggregate> courses, CancellationToken ct = default)
    {
        try
        {
            _dbContext.Courses.UpdateRange(courses);
            return Task.CompletedTask;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error when updating courses range");
            throw new RepositoryException("Không thể cập nhật khóa học", ex);
        }
         catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating courses range");
            throw;
        }
    }
}

