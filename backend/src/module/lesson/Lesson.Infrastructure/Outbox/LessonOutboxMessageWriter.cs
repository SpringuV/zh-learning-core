using HanziAnhVu.Shared.Application;
using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Lesson.Infrastructure.Outbox;

public class LessonOutboxMessageWriter(LessonDbContext dbContext) : IOutboxWriter
{
    private readonly LessonDbContext _dbContext = dbContext;

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<LessonModuleOutboxMessage>(integrationEvent);
        _dbContext.LessonOutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}