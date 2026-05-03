namespace Lesson.Infrastructure.Repository;

using Npgsql;
using NpgsqlTypes;

// noi save se la unit of work
public class CourseRepository(LessonDbContext dbContext, ILogger<CourseRepository> logger) : LessonRepositoryBase(logger), ICourseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private const int ReorderTempBase = 1_000_000;

    public async Task AddAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                // Chi them vao DbSet, khong save - de UnitOfWork xu ly
                await _dbContext.Courses.AddAsync(course, cancellationToken);
            },
            "Database error when adding course: {CourseId}",
            "Unexpected error adding course",
            "Không thể thêm khóa học vào database",
            course.CourseId);
    }

    public async Task<CourseAggregate?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        // FindAsync se tim kiem theo khoa chinh (primary key) cua bang, o day la Id cua CourseAggregate.
        return await _dbContext.Courses.FindAsync([courseId], cancellationToken);
    }

    public async Task UpdateAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            () =>
            {
                _dbContext.Courses.Update(course);
                // await _dbContext.SaveChangesAsync(cancellationToken); - de UnitOfWork xu ly
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
                    // await _dbContext.SaveChangesAsync(cancellationToken); - de UnitOfWork xu ly
                }
            },
            "Database error when deleting course: {CourseId}",
            "Unexpected error deleting course",
            "Không thể xóa khóa học khỏi database",
            courseId);
    }

    public async Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                return await _dbContext.Courses.Select(c => (int?)c.OrderIndex).DefaultIfEmpty(0).MaxAsync(ct);
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
                // Su dung Where de loc cac khoa hoc co CourseId nam trong danh sach ids,
                // sau do chuyen ket qua thanh List, no se chay mot truy van SQL voi dieu kien IN
                // de lay tat ca cac khoa hoc co ID khop trong mot lan truy van.
                return await _dbContext.Courses.Where(c => ids.Contains(c.CourseId)).ToListAsync(ct);
            },
            "Database error getting courses by IDs",
            "Unexpected error getting courses by IDs",
            "Không thể lấy các khóa học theo danh sách ID");
    }

    public Task UpdateRangeAsync(IEnumerable<CourseAggregate> courses, CancellationToken ct = default)
    {
        return ExecuteAsync(
            () =>
            {
                _dbContext.Courses.UpdateRange(courses); // UpdateRange se danh dau tat ca cac entity
                // trong danh sach la da duoc sua doi (Modified)
                // va se cap nhat chung trong database khi SaveChangesAsync duoc goi.
            },
            "Database error when updating courses range",
            "Unexpected error updating courses range",
            "Không thể cập nhật các khóa học",
            string.Join(", ", courses.Select(c => c.CourseId)));
    }

    public async Task ReorderByIdsAsync(IReadOnlyList<Guid> orderedCourseIds, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                if (orderedCourseIds.Count == 0)
                    return;

                var idsParameter = new NpgsqlParameter<Guid[]>("ids", orderedCourseIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameter = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToTempSql = @"
                    WITH input AS (
                        SELECT u.""CourseId"", u.""Position""::int AS ""FinalOrder""
                        FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""CourseId"", ""Position"")
                    ),
                    selected AS (
                        SELECT c.""CourseId"", c.""OrderIndex""
                        FROM ""Courses"" c
                        JOIN input i ON c.""CourseId"" = i.""CourseId""
                    ),
                    guards AS (
                        SELECT
                            (SELECT COUNT(*)::int FROM input) AS ""InputCount"",
                            (SELECT COUNT(DISTINCT ""CourseId"")::int FROM input) AS ""DistinctInputCount"",
                            (SELECT COUNT(*)::int FROM selected) AS ""MatchedCourses"",
                            (SELECT COALESCE(MAX(c.""OrderIndex""), 0)::int FROM ""Courses"" c) AS ""MaxOrderIndex""
                    )
                    UPDATE ""Courses"" AS c
                    SET ""OrderIndex"" = @tempBase + s.""OrderIndex"",
                        ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
                    FROM selected s
                    CROSS JOIN guards g
                    WHERE c.""CourseId"" = s.""CourseId""
                    AND g.""InputCount"" = g.""DistinctInputCount""
                    AND g.""MatchedCourses"" = g.""InputCount""
                    AND g.""MaxOrderIndex"" < @tempBase
                    ";

                var movedToTempCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToTempSql, [idsParameter, tempBaseParameter], ct);

                if (movedToTempCount != orderedCourseIds.Count)
                    throw new ArgumentException("Danh sach sap xep phai chua ID duy nhat va tat ca ID phai ton tai.");

                var idsParameterFinal = new NpgsqlParameter<Guid[]>("ids", orderedCourseIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameterFinal = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToFinalSql = @"
                    WITH input AS (
                        SELECT u.""CourseId"", u.""Position""::int AS ""FinalOrder""
                        FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""CourseId"", ""Position"")
                    ),
                    selected AS (
                        SELECT c.""CourseId"", (c.""OrderIndex"" - @tempBase)::int AS ""OriginalOrderIndex""
                        FROM ""Courses"" c
                        JOIN input i ON c.""CourseId"" = i.""CourseId""
                    ),
                    slots AS (
                        SELECT
                            ROW_NUMBER() OVER (ORDER BY s.""OriginalOrderIndex"")::int AS ""SlotPosition"",
                            s.""OriginalOrderIndex"" AS ""TargetOrderIndex""
                        FROM selected s
                    ),
                    guards AS (
                        SELECT
                            (SELECT COUNT(*)::int FROM input) AS ""InputCount"",
                            (SELECT COUNT(DISTINCT ""CourseId"")::int FROM input) AS ""DistinctInputCount"",
                            (SELECT COUNT(*)::int FROM selected) AS ""MatchedCourses"",
                            (SELECT COUNT(*)::int FROM selected s WHERE s.""OriginalOrderIndex"" > 0) AS ""ValidOriginalOrderCount""
                    )
                    UPDATE ""Courses"" AS c
                    SET ""OrderIndex"" = s.""TargetOrderIndex"",
                        ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
                    FROM input i
                    CROSS JOIN guards g
                    JOIN slots s ON s.""SlotPosition"" = i.""FinalOrder""
                    WHERE c.""CourseId"" = i.""CourseId""
                    AND g.""InputCount"" = g.""DistinctInputCount""
                    AND g.""MatchedCourses"" = g.""InputCount""
                    AND g.""ValidOriginalOrderCount"" = g.""InputCount""
                    ";

                var updatedCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToFinalSql, [idsParameterFinal, tempBaseParameterFinal], ct);

                if (updatedCount != orderedCourseIds.Count)
                    throw new ArgumentException("Danh sach sap xep phai chua ID duy nhat va tat ca ID phai ton tai.");
            },
            "Database error when reordering courses",
            "Unexpected error reordering courses",
            "Không thể sắp xếp lại các khóa học",
            string.Join(", ", orderedCourseIds));
    }

    public async Task<int> GetHskLevelByCourseIdAsync(Guid courseId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                var course = await GetByIdAsync(courseId, ct) ?? throw new KeyNotFoundException($"Course with ID {courseId} not found.");;
                return course.HskLevel;
            },
            "Database error when getting HSK level for course: {CourseId}",
            "Unexpected error getting HSK level for course",
            "Không thể lấy mức HSK cho khóa học",
            courseId);
    }

}
