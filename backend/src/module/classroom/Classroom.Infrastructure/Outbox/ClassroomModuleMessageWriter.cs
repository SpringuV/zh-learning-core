using HanziAnhVu.Shared.Application;
using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Classroom.Infrastructure.Outbox;

public class ClassroomModuleMessageWriter(ClassroomDbContext dbContext) : IOutboxWriter
{
    private readonly ClassroomDbContext _dbContext = dbContext;

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<ClassroomModuleOutboxMessage>(integrationEvent);
        _dbContext.ClassroomOutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}