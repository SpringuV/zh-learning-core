namespace Lesson.Infrastructure.Repository;

using Microsoft.Extensions.Caching.Distributed;
using Npgsql;
using NpgsqlTypes;

public class TopicRepository(
    LessonDbContext dbContext,
    IDistributedCache distributedCache,
    ICacheVersionService cacheVersionService,
    ILogger<TopicRepository> logger) : LessonRepositoryBase(logger), ITopicRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private const int ReorderTempBase = 1_000_000_000;
    private static readonly DistributedCacheEntryOptions TopicCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    };

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

            await InvalidateTopicCacheAsync(id, ct);
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

    public async Task<TopicAggregate?> GetByIdAsync(Guid topicId, CancellationToken ct = default)
    {
        if (topicId == Guid.Empty)
        {
            return null;
        }

        var cacheKey = await BuildTopicCacheKeyAsync(topicId, ct);

        try
        {
            var cachedBytes = await _distributedCache.GetAsync(cacheKey, ct);
            if (cachedBytes is { Length: > 0 })
            {
                var cachedTopic = JsonSerializer.Deserialize<TopicAggregate>(cachedBytes);
                if (cachedTopic is not null)
                {
                    return cachedTopic;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to read topic cache for {TopicId}", topicId);
        }

        var topic = await ExecuteAsync(
            async () => await _dbContext.Topics
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TopicId == topicId, ct),
            "Database error retrieving topic: {TopicId}",
            "Unexpected error retrieving topic",
            "Không thể truy xuất topic từ database",
            topicId);

        if (topic is null)
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(topic);
            await _distributedCache.SetAsync(cacheKey, payload, TopicCacheOptions, ct);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to store topic cache for {TopicId}", topicId);
        }

        return topic;
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

    private async Task ReorderByIdsAndCourseIdAsyncLegacy(Guid courseId, IReadOnlyList<Guid> orderedTopicIds, CancellationToken ct = default)
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
                // cache kết quả OrderIndex tối đa vì nó thường được gọi khi tạo topic mới để xác định OrderIndex của topic mới sẽ là bao nhiêu (thường là max + 1)
                var cacheKey = "MaxOrderIndexTopic";
                try
                {
                    var cachedBytes = await _distributedCache.GetAsync(cacheKey, ct);
                    if (cachedBytes is { Length: > 0 })
                    {
                        var cachedValue = BitConverter.ToInt32(cachedBytes, 0);
                        return cachedValue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to read max order index topic cache");
                }

                // Nếu cache miss, truy vấn database để lấy max OrderIndex
                var maxOrderIndex = await _dbContext.Topics
                        .Select(t => (int?)t.OrderIndex)
                        .MaxAsync(ct);
                try
                {
                    var bytes = BitConverter.GetBytes(maxOrderIndex ?? -1);
                    await _distributedCache.SetAsync(cacheKey, bytes, TopicCacheOptions, ct);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to set max order index topic cache");
                }
        
                return maxOrderIndex;
                    // Keep projection nullable so empty Topics table returns null instead of throwing.
            },
            "Database error getting max OrderIndex",
            "Error getting max OrderIndex",
            "Không thể lấy OrderIndex tối đa"
        );
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

        await InvalidateTopicCacheAsync(topic.TopicId, ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<TopicAggregate> topics, CancellationToken ct = default)
    {
        var topicList = topics.ToList();

        await ExecuteAsync(
            () =>
            {
                _dbContext.Topics.UpdateRange(topicList);
                // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            },
            "Database error when updating topics",
            "Unexpected error updating topics",
            "Không thể cập nhật các topic");

        foreach (var topicId in topicList.Select(topic => topic.TopicId).Distinct())
        {
            await InvalidateTopicCacheAsync(topicId, ct);
        }
    }

    public async Task ReorderByIdsAndCourseIdAsync(Guid courseId, IReadOnlyList<Guid> orderedTopicIds, CancellationToken ct = default)
    {
        await ReorderByIdsAndCourseIdAsyncLegacy(courseId, orderedTopicIds, ct);

        foreach (var topicId in orderedTopicIds.Distinct())
        {
            await InvalidateTopicCacheAsync(topicId, ct);
        }
    }

    private async Task<string> BuildTopicCacheKeyAsync(Guid topicId, CancellationToken ct)
    {
        var scope = BuildTopicCacheScope(topicId);
        var versionToken = await _cacheVersionService.GetVersionTokenAsync(scope, ct);
        return $"{scope}:v:{versionToken}";
    }

    private async Task InvalidateTopicCacheAsync(Guid topicId, CancellationToken ct)
    {
        try
        {
            await _cacheVersionService.InvalidateScopeAsync(BuildTopicCacheScope(topicId), ct);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to invalidate topic cache for {TopicId}", topicId);
        }
    }

    private static string BuildTopicCacheScope(Guid topicId)
        => $"lesson:topic:by-id:{topicId:D}";
}