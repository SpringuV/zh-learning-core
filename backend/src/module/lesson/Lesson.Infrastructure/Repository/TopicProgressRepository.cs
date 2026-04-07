using Lesson.Domain.Entities;
using Lesson.Domain.Entities.Exercise;
using Lesson.Domain.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lesson.Infrastructure.Repository;

public class TopicProgressRepository(LessonDbContext dbContext, ILogger<TopicProgressRepository> logger, IPublisher publisher) : ITopicProgressRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<TopicProgressRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public Task<IEnumerable<TopicProgressAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    public async Task AddAsync(TopicProgressAggregate course, CancellationToken cancellationToken = default)
    {
        // AddAsync sẽ thêm một entity mới vào DbSet và đánh dấu nó là "Added". 
        // Khi SaveChangesAsync được gọi, EF Core sẽ thực hiện một câu lệnh INSERT 
        // để thêm bản ghi mới vào database.
        await _dbContext.TopicProgresses.AddAsync(course, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseAggregate?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        // FindAsync sẽ tìm kiếm theo khóa chính (primary key) của bảng, ở đây là Id của CourseAggregate.
        return await _dbContext.Courses.FindAsync([courseId], cancellationToken);
    }

    public async Task UpdateAsync(CourseAggregate course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Update(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await GetByIdAsync(courseId, cancellationToken);
        if (course != null)
        {
            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SaveAsync(CourseAggregate course, CancellationToken ct = default)
    {
        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(ct);
        
        // Publish events after save
        var events = course.PopDomainEvents();
        foreach(var evt in events)
            await _publisher.Publish(evt, ct);
    }

    Task<TopicProgressAggregate?> ITopicProgressRepository.GetByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TopicProgressAggregate topicProgress, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}