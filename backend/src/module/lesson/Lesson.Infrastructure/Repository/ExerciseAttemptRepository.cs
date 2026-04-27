

namespace Lesson.Infrastructure.Repository;

public class ExerciseAttemptRepository(ICacheVersionService cacheVersionService, IDistributedCache cache, LessonDbContext dbContext, ILogger<ExerciseAttemptRepository> logger) : LessonRepositoryBase(logger), IExerciseAttemptRepository
{
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly IDistributedCache _cache = cache;
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<ExerciseAttemptRepository> _logger = logger;
    private static readonly DistributedCacheEntryOptions TopicCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    };

    #region Add
    public async Task AddAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.ExerciseAttempts.AddAsync(exerciseAttempt, ct);
            },
            "Database error while adding exercise attempt with id {ExerciseAttemptId}",
            "Unexpected error while adding exercise attempt with id {ExerciseAttemptId}",
            "Failed to add exercise attempt",
            exerciseAttempt.AttemptId
        );
    }
    #endregion
    #region Delete
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                var entity = await GetByIdAsync(id, ct);
                if (entity != null)
                {
                    _dbContext.ExerciseAttempts.Remove(entity);
                }
            },
            "Database error while deleting exercise attempt with id {ExerciseAttemptId}",
            "Unexpected error while deleting exercise attempt with id {ExerciseAttemptId}",
            "Failed to delete exercise attempt",
            id
        );
        await InvalidateCacheAsync(id, ct);
    }
    #endregion
    #region GetExerciseIdAndSessionId
    public async Task<IEnumerable<ExerciseAttemptAggregate>> GetAllBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                return await _dbContext.ExerciseAttempts.Where(ea => ea.SessionId == sessionId).ToListAsync(ct);
            },
            "Database error while retrieving exercise attempts for session id {SessionId}",
            "Unexpected error while retrieving exercise attempts for session id {SessionId}",
            "Failed to retrieve exercise attempts for session",
            sessionId
        );
    }
    #endregion
    #region GetExerIdAndSessionId
    public async Task<ExerciseAttemptAggregate?> GetByExerciseIdAndSessionIdAsync(Guid exerciseId, Guid sessionId, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                return await _dbContext.ExerciseAttempts.FirstOrDefaultAsync(ea => ea.ExerciseId == exerciseId && ea.SessionId == sessionId, ct);
            },
            "Database error while retrieving exercise attempt with id {ExerciseAttemptId} and session id {SessionId}",
            "Unexpected error while retrieving exercise attempt with id {ExerciseAttemptId} and session id {SessionId}",
            "Failed to retrieve exercise attempt",
            exerciseId
        );
    }
    #endregion

    #region GetByIdCache
    public async Task<ExerciseAttemptAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async () =>
            {
                var cacheKey = await BuildCacheKeyAsync(id, ct);
                try
                {
                    var cachedData = await _cache.GetStringAsync(cacheKey, ct);
                    if (cachedData != null)
                    {
                        var cachedAttempt = JsonSerializer.Deserialize<ExerciseAttemptAggregate>(cachedData);
                        if (cachedAttempt != null)
                        {
                            return cachedAttempt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while accessing cache for exercise attempt with id {ExerciseAttemptId}", id);
                }
                // For simplicity, using FindAsync which will look in the cache first before hitting the database
                var exerciseAttempt = await _dbContext.ExerciseAttempts.FindAsync([id], ct);
                if (exerciseAttempt != null)
                {
                    try
                    {
                        var serializedData = JsonSerializer.Serialize(exerciseAttempt);
                        await _cache.SetStringAsync(cacheKey, serializedData, TopicCacheOptions, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while setting cache for exercise attempt with id {ExerciseAttemptId}", id);
                    }
                }
                return exerciseAttempt;
            },
            "Database error while retrieving exercise attempt with id {ExerciseAttemptId}",
            "Unexpected error while retrieving exercise attempt with id {ExerciseAttemptId}",
            "Failed to retrieve exercise attempt",
            id
        );
    }
    #endregion

    #region Update
    public async Task UpdateAsync(ExerciseAttemptAggregate exerciseAttempt, CancellationToken ct = default)
    {
        await ExecuteAsync(
            async () =>
            {
                _dbContext.ExerciseAttempts.Update(exerciseAttempt);
            },
            "Database error while updating exercise attempt with id {ExerciseAttemptId}",
            "Unexpected error while updating exercise attempt with id {ExerciseAttemptId}",
            "Failed to update exercise attempt",
            exerciseAttempt.AttemptId
        );
        await InvalidateCacheAsync(exerciseAttempt.AttemptId, ct);
    }
    #endregion

    #region HelperMethods
    private async Task<string> BuildCacheKeyAsync(Guid exerciseAttemptId, CancellationToken ct = default)
    {
        var cacheScope = BuildCacheScope(exerciseAttemptId);
        var version = await _cacheVersionService.GetVersionTokenAsync(cacheScope, ct);
        return $"attempt:{cacheScope}:v{version}";
    }
    private async Task InvalidateCacheAsync(Guid exerciseAttemptId, CancellationToken ct = default)
    {
        try
        {
            await _cacheVersionService.InvalidateScopeAsync(BuildCacheScope(exerciseAttemptId), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while invalidating cache for exercise attempt with id {ExerciseAttemptId}", exerciseAttemptId);
        }
    }
    private static string BuildCacheScope(Guid exerciseAttemptId)
        => $"lesson:ea:by-id:{exerciseAttemptId:D}";
    #endregion
}