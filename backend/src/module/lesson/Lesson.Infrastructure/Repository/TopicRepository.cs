using Lesson.Domain.Entities;
using Lesson.Domain.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lesson.Infrastructure.Repository;

public class TopicRepository(LessonDbContext dbContext, ILogger<TopicRepository> logger, IPublisher publisher) : ITopicRepository
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<TopicRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public Task AddAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TopicAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TopicAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TopicAggregate topic, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}