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

    public async Task<bool> ExistsOrderIndexAsync(int orderIndex, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Courses.AnyAsync(c => c.OrderIndex == orderIndex, cancellationToken);
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

}

