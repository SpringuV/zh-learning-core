using Lesson.Domain.Entities.Exercise;
using Lesson.Domain.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lesson.Infrastructure.Repository;

public class UserTopicExerciseSessionRepository(LessonDbContext dbContext, ILogger<UserTopicExerciseSessionRepository> logger, IPublisher publisher) : IUserTopicExerciseSessionRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<UserTopicExerciseSessionRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public Task<IEnumerable<UserTopicExerciseSessionAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    public async Task AddAsync(UserTopicExerciseSessionAggregate session, CancellationToken cancellationToken = default)
    {
        // AddAsync sẽ thêm một entity mới vào DbSet và đánh dấu nó là "Added". 
        // Khi SaveChangesAsync được gọi, EF Core sẽ thực hiện một câu lệnh INSERT 
        // để thêm bản ghi mới vào database.
        await _dbContext.UserExerciseSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserTopicExerciseSessionAggregate?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của UserTopicExerciseSessionAggregate.
        return await _dbContext.UserExerciseSessions.FindAsync([sessionId], cancellationToken);
    }

    public async Task UpdateAsync(UserTopicExerciseSessionAggregate session, CancellationToken cancellationToken = default)
    {
        _dbContext.UserExerciseSessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await GetByIdAsync(sessionId, cancellationToken);
        if (session != null)
        {
            _dbContext.UserExerciseSessions.Remove(session);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SaveAsync(UserTopicExerciseSessionAggregate session, CancellationToken ct = default)
    {
        _dbContext.UserExerciseSessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);
        
        // Publish events after save
        var events = session.PopDomainEvents();
        foreach(var evt in events)
            await _publisher.Publish(evt, ct);
    }
}