namespace Lesson.Infrastructure.Repository;

// nơi save sẽ là unit of work
public class CourseRepository(LessonDbContext dbContext, ILogger<CourseRepository> logger) : LessonRepositoryBase(logger), ICourseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;

    public async Task AddAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                // Chỉ thêm vào DbSet, không save - để UnitOfWork xử lý
                await _dbContext.Courses.AddAsync(course, cancellationToken);
            },
            "Database error when adding course: {CourseId}",
            "Unexpected error adding course",
            "Không thể thêm khóa học vào database",
            course.CourseId);
    }

    public async Task<CourseAggregate?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của CourseAggregate.
        return await _dbContext.Courses.FindAsync([courseId], cancellationToken);
    }

    public async Task UpdateAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            () =>
            {
                _dbContext.Courses.Update(course);
                // await _dbContext.SaveChangesAsync(cancellationToken); - để UnitOfWork xử lý
            },
            "Database error when updating course: {CourseId}",
            "Unexpected error updating course",
            "Không thể cập nhật khóa học",
            course.CourseId);
    }

    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                var course = await GetByIdAsync(courseId, cancellationToken);
                if (course != null)
                {
                    _dbContext.Courses.Remove(course);
                    // await _dbContext.SaveChangesAsync(cancellationToken); - để UnitOfWork xử lý
                }
            },
            "Database error when deleting course: {CourseId}",
            "Unexpected error deleting course",
            "Không thể xóa khóa học khỏi database",
            courseId);
    }

    public async Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default)
    {
        return await ExecuteAsync( async ()=>
        {
            return await _dbContext.Courses.Select(c => (int?)c.OrderIndex ?? 0).MaxAsync(ct);
        },
        "Database error getting max OrderIndex",
        "Unexpected error getting max OrderIndex",
        "Không thể lấy OrderIndex tối đa");
    }

    public async Task<IEnumerable<CourseAggregate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                // Sử dụng Where để lọc các khóa học có CourseId nằm trong danh sách ids, 
                // sau đó chuyển kết quả thành List, nó sẽ chạy một truy vấn SQL với điều kiện IN 
                // để lấy tất cả các khóa học có ID khớp trong một lần truy vấn.
                return await _dbContext.Courses.Where(c => ids.Contains(c.CourseId)).ToListAsync(ct);
            },
            "Database error getting courses by IDs",
            "Unexpected error getting courses by IDs",
            "Không thể lấy khóa học theo IDs");
    }

    public Task UpdateRangeAsync(IEnumerable<CourseAggregate> courses, CancellationToken ct = default)
    {
        return ExecuteAsync(
            () =>
            {
                _dbContext.Courses.UpdateRange(courses); // UpdateRange sẽ đánh dấu tất cả các entity
                //  trong danh sách là đã được sửa đổi (Modified) 
                // và sẽ cập nhật chúng trong database khi SaveChangesAsync được gọi.
            },
            "Database error when updating courses range",
            "Unexpected error updating courses range",
            "Không thể cập nhật khóa học",
            string.Join(", ", courses.Select(c => c.CourseId)));
    }
}