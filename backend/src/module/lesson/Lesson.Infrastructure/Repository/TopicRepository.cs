namespace Lesson.Infrastructure.Repository;

using Npgsql;
using NpgsqlTypes;

public class TopicRepository(LessonDbContext dbContext, ILogger<TopicRepository> logger) : LessonRepositoryBase(logger), ITopicRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private const int ReorderTempBase = 1_000_000_000;

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

    public async Task ReorderByIdsAndCourseIdAsync(Guid courseId, IReadOnlyList<Guid> orderedTopicIds, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                if (orderedTopicIds.Count == 0)
                    return;

                var courseIdParameter = new NpgsqlParameter<Guid>("courseId", courseId)
                {
                    NpgsqlDbType = NpgsqlDbType.Uuid
                };

                var idsParameter = new NpgsqlParameter<Guid[]>("ids", orderedTopicIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameter = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToTempSql = @"
WITH input AS (
    SELECT u.""TopicId"", u.""Position""::int AS ""FinalOrder""
    FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""TopicId"", ""Position"")
),
selected AS (
    SELECT t.""TopicId"", t.""OrderIndex""
    FROM ""Topics"" t
    JOIN input i ON t.""TopicId"" = i.""TopicId""
    WHERE t.""CourseId"" = @courseId
),
guards AS (
    SELECT
        (SELECT COUNT(*)::int FROM input) AS ""InputCount"",
        (SELECT COUNT(DISTINCT ""TopicId"")::int FROM input) AS ""DistinctInputCount"",
        (SELECT COUNT(*)::int FROM selected) AS ""MatchedTopicsInCourse"",
        (SELECT COALESCE(MAX(t.""OrderIndex""), 0)::int FROM ""Topics"" t WHERE t.""CourseId"" = @courseId) AS ""MaxOrderIndexInCourse""
)
UPDATE ""Topics"" AS t
SET ""OrderIndex"" = @tempBase + s.""OrderIndex"",
    ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
FROM selected s
CROSS JOIN guards g
WHERE t.""TopicId"" = s.""TopicId""
  AND t.""CourseId"" = @courseId
  AND g.""InputCount"" = g.""DistinctInputCount""
  AND g.""MatchedTopicsInCourse"" = g.""InputCount""
  AND g.""MaxOrderIndexInCourse"" < @tempBase
";

                var movedToTempCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToTempSql, [courseIdParameter, idsParameter, tempBaseParameter], ct);

                if (movedToTempCount != orderedTopicIds.Count)
                    throw new ArgumentException("Danh sách sắp xếp phải chứa ID duy nhất và tất cả ID phải thuộc khóa học tương ứng.");

                var idsParameterFinal = new NpgsqlParameter<Guid[]>("ids", orderedTopicIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameterFinal = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToFinalSql = @"
WITH input AS (
    SELECT u.""TopicId"", u.""Position""::int AS ""FinalOrder""
    FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""TopicId"", ""Position"")
),
selected AS (
    SELECT t.""TopicId"", (t.""OrderIndex"" - @tempBase)::int AS ""OriginalOrderIndex""
    FROM ""Topics"" t
    JOIN input i ON t.""TopicId"" = i.""TopicId""
    WHERE t.""CourseId"" = @courseId
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
        (SELECT COUNT(DISTINCT ""TopicId"")::int FROM input) AS ""DistinctInputCount"",
        (SELECT COUNT(*)::int FROM selected) AS ""MatchedTopicsInCourse"",
        (SELECT COUNT(*)::int FROM selected s WHERE s.""OriginalOrderIndex"" > 0) AS ""ValidOriginalOrderCount""
)
UPDATE ""Topics"" AS t
SET ""OrderIndex"" = s.""TargetOrderIndex"",
    ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
FROM input i
CROSS JOIN guards g
JOIN slots s ON s.""SlotPosition"" = i.""FinalOrder""
WHERE t.""TopicId"" = i.""TopicId""
  AND t.""CourseId"" = @courseId
  AND g.""InputCount"" = g.""DistinctInputCount""
  AND g.""MatchedTopicsInCourse"" = g.""InputCount""
  AND g.""ValidOriginalOrderCount"" = g.""InputCount""
";

                var updatedCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToFinalSql, [courseIdParameter, idsParameterFinal, tempBaseParameterFinal], ct);

                if (updatedCount != orderedTopicIds.Count)
                    throw new ArgumentException("Danh sách sắp xếp phải chứa ID duy nhất và tất cả ID phải thuộc khóa học tương ứng.");
            },
            "Database error when reordering topics. CourseId: {CourseId}",
            "Unexpected error reordering topics",
            "Không thể sắp xếp lại thứ tự chủ đề",
            courseId);
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