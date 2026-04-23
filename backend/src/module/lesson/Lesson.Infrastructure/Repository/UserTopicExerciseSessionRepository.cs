namespace Lesson.Infrastructure.Repository;

public class UserTopicExerciseSessionRepository(LessonDbContext dbContext, ILogger<UserTopicExerciseSessionRepository> logger) : LessonRepositoryBase(logger), IUserTopicExerciseSessionRepository
{
    private readonly LessonDbContext _dbContext = dbContext;

    public async Task AddAsync(UserTopicExerciseSessionAggregate session, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                await _dbContext.UserExerciseSessions.AddAsync(session, cancellationToken);
                // UnitOfWork sẽ gọi SaveChangesAsync.
            },
            "Database error when adding session: {SessionId}",
            "Unexpected error adding session",
            "Không thể thêm session vào database",
            session.SessionId);
    }

    public async Task<UserTopicExerciseSessionAggregate?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserExerciseSessions.FindAsync([sessionId], cancellationToken);
    }

    public async Task UpdateAsync(UserTopicExerciseSessionAggregate session, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                _dbContext.UserExerciseSessions.Update(session);
                // UnitOfWork sẽ gọi SaveChangesAsync.
            },
            "Database error when updating session: {SessionId}",
            "Unexpected error updating session",
            "Không thể cập nhật session",
            session.SessionId);
    }

    public async Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(
            async () =>
            {
                var session = await GetByIdAsync(sessionId, cancellationToken);
                if (session != null)
                {
                    _dbContext.UserExerciseSessions.Remove(session);
                    // UnitOfWork sẽ gọi SaveChangesAsync.
                }
            },
            "Database error when deleting session: {SessionId}",
            "Unexpected error deleting session",
            "Không thể xóa session khỏi database",
            sessionId);
    }
}