namespace Lesson.Infrastructure.Repository;

using Npgsql;
using NpgsqlTypes;

public class ExerciseRepository(
        LessonDbContext dbContext, 
        ILogger<ExerciseRepository> logger) 
    : LessonRepositoryBase(logger), IExerciseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private const int ReorderTempBase = 1_000_000_000;

    public async Task AddAsync(ExerciseAggregate exercise, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.Exercises.AddAsync(exercise, ct);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when adding exercise: {ExerciseId}",
            "Unexpected error adding exercise",
            "Không thể thêm exercise vào database",
            exercise.ExerciseId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var exercise = await GetByIdAsync(id, ct);
        if (exercise == null)
        {
            Logger.LogWarning("Exercise with ID {ExerciseId} not found for deletion", id);
            return; // Or throw an exception if you prefer
        }

        await ExecuteAsync(
            () =>
            {
                _dbContext.Exercises.Remove(exercise);
                // await _dbContext.SaveChangesAsync(ct); - để UnitOfWork xử lý
            },
            "Database error when deleting exercise: {ExerciseId}",
            "Unexpected error deleting exercise",
            "Không thể xóa exercise khỏi database",
            id);
    }

    public async Task<ExerciseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Exercises.FindAsync([id], ct),
            "Database error when retrieving exercise: {ExerciseId}",
            "Unexpected error retrieving exercise",
            "Không thể truy xuất exercise từ database",
            id);
    }

    public async Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAndIdsAsync(Guid topicId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Exercises
                .Where(e => e.TopicId == topicId && ids.Contains(e.ExerciseId))
                .ToListAsync(ct),
            "Database error retrieving exercises by topic ID and exercise IDs: {TopicId}, {ExerciseIds}",
            "Unexpected error retrieving exercises by topic ID and exercise IDs: {TopicId}, {ExerciseIds}",
            "Không thể truy xuất exercises theo topic ID và exercise IDs",
            topicId, string.Join(",", ids)
        );
    }

    public async Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAsync(Guid topicId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Exercises.Where(e => e.TopicId == topicId).ToListAsync(ct),
            "Database error retrieving exercises by topic ID: {TopicId}",
            "Unexpected error retrieving exercises by topic ID: {TopicId}",
            "Không thể truy xuất exercises theo topic ID",
            topicId);
    }

    public async Task ReorderByIdsAndTopicIdAsync(Guid topicId, IReadOnlyList<Guid> orderedExerciseIds, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                if (orderedExerciseIds.Count == 0)
                    return;

                var topicIdParameter = new NpgsqlParameter<Guid>("topicId", topicId)
                {
                    NpgsqlDbType = NpgsqlDbType.Uuid
                };

                var idsParameter = new NpgsqlParameter<Guid[]>("ids", orderedExerciseIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameter = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToTempSql = @"
WITH input AS (
    SELECT u.""ExerciseId"", u.""Position""::int AS ""FinalOrder""
    FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""ExerciseId"", ""Position"")
),
selected AS (
    SELECT e.""ExerciseId"", e.""OrderIndex""
    FROM ""Exercises"" e
    JOIN input i ON e.""ExerciseId"" = i.""ExerciseId""
    WHERE e.""TopicId"" = @topicId
),
guards AS (
    SELECT
        (SELECT COUNT(*)::int FROM input) AS ""InputCount"",
        (SELECT COUNT(DISTINCT ""ExerciseId"")::int FROM input) AS ""DistinctInputCount"",
        (SELECT COUNT(*)::int FROM selected) AS ""MatchedExercisesInTopic"",
        (SELECT COALESCE(MAX(e.""OrderIndex""), 0)::int FROM ""Exercises"" e WHERE e.""TopicId"" = @topicId) AS ""MaxOrderIndexInTopic""
)
UPDATE ""Exercises"" AS e
SET ""OrderIndex"" = @tempBase + s.""OrderIndex"",
    ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
FROM selected s
CROSS JOIN guards g
WHERE e.""ExerciseId"" = s.""ExerciseId""
  AND e.""TopicId"" = @topicId
  AND g.""InputCount"" = g.""DistinctInputCount""
  AND g.""MatchedExercisesInTopic"" = g.""InputCount""
  AND g.""MaxOrderIndexInTopic"" < @tempBase
";

                var movedToTempCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToTempSql, [topicIdParameter, idsParameter, tempBaseParameter], ct);

                if (movedToTempCount != orderedExerciseIds.Count)
                    throw new ArgumentException("Danh sách sắp xếp phải chứa ID duy nhất và tất cả ID phải thuộc chủ đề tương ứng.");

                var idsParameterFinal = new NpgsqlParameter<Guid[]>("ids", orderedExerciseIds.ToArray())
                {
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                };

                var tempBaseParameterFinal = new NpgsqlParameter<int>("tempBase", ReorderTempBase)
                {
                    NpgsqlDbType = NpgsqlDbType.Integer
                };

                const string moveToFinalSql = @"
WITH input AS (
    SELECT u.""ExerciseId"", u.""Position""::int AS ""FinalOrder""
    FROM unnest(@ids::uuid[]) WITH ORDINALITY AS u(""ExerciseId"", ""Position"")
),
selected AS (
    SELECT e.""ExerciseId"", (e.""OrderIndex"" - @tempBase)::int AS ""OriginalOrderIndex""
    FROM ""Exercises"" e
    JOIN input i ON e.""ExerciseId"" = i.""ExerciseId""
    WHERE e.""TopicId"" = @topicId
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
        (SELECT COUNT(DISTINCT ""ExerciseId"")::int FROM input) AS ""DistinctInputCount"",
        (SELECT COUNT(*)::int FROM selected) AS ""MatchedExercisesInTopic"",
        (SELECT COUNT(*)::int FROM selected s WHERE s.""OriginalOrderIndex"" > 0) AS ""ValidOriginalOrderCount""
)
UPDATE ""Exercises"" AS e
SET ""OrderIndex"" = s.""TargetOrderIndex"",
    ""UpdatedAt"" = NOW() AT TIME ZONE 'UTC'
FROM input i
CROSS JOIN guards g
JOIN slots s ON s.""SlotPosition"" = i.""FinalOrder""
WHERE e.""ExerciseId"" = i.""ExerciseId""
  AND e.""TopicId"" = @topicId
  AND g.""InputCount"" = g.""DistinctInputCount""
  AND g.""MatchedExercisesInTopic"" = g.""InputCount""
  AND g.""ValidOriginalOrderCount"" = g.""InputCount""
";

                var updatedCount = await _dbContext.Database
                    .ExecuteSqlRawAsync(moveToFinalSql, [topicIdParameter, idsParameterFinal, tempBaseParameterFinal], ct);

                if (updatedCount != orderedExerciseIds.Count)
                    throw new ArgumentException("Danh sách sắp xếp phải chứa ID duy nhất và tất cả ID phải thuộc chủ đề tương ứng.");
            },
            "Database error when reordering exercises. TopicId: {TopicId}",
            "Unexpected error reordering exercises",
            "Không thể sắp xếp lại thứ tự bài tập",
            topicId);
    }

    public async Task<int?> GetMaxOrderIndexByTopicIdAsync(Guid topicId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () => await _dbContext.Exercises
                .Where(e => e.TopicId == topicId)
                .Select(e => (int?)e.OrderIndex)
                .MaxAsync(ct),
            "Database error getting max OrderIndex",
            "Error getting max OrderIndex",
            "Không thể lấy OrderIndex tối đa");
    }

    public Task UpdateAsync(ExerciseAggregate exercise, CancellationToken ct = default)
    {
        return ExecuteAsync(
            () =>
            {
                _dbContext.Exercises.Update(exercise);
                // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            },
            "Database error when updating exercise: {ExerciseId}",
            "Unexpected error updating exercise",
            "Không thể cập nhật exercise",
            exercise.ExerciseId);
    }

    public Task UpdateRangeAsync(IEnumerable<ExerciseAggregate> exercises, CancellationToken ct = default)
    {
        return ExecuteAsync(
            () =>
            {
                _dbContext.Exercises.UpdateRange(exercises);
                // await _dbContext.SaveChangesAsync(ct); // Để UnitOfWork xử lý SaveChangesAsync
            },
            "Database error when updating exercises",
            "Unexpected error updating exercises",
            "Không thể cập nhật các exercise");
    }
}