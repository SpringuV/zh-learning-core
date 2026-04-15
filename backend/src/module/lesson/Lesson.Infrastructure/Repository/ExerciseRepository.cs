namespace Lesson.Infrastructure.Repository;

public class ExerciseRepository(
        LessonDbContext dbContext, 
        ILogger<ExerciseRepository> logger) 
    : LessonRepositoryBase(logger), IExerciseRepository
{
    private readonly LessonDbContext _dbContext = dbContext;

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