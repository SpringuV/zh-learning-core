using HanziAnhVu.Shared.Application;
using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Users.Infrastructure.Outbox;

public class UserOutboxMessageWriter(UserModuleDbContext dbContext) : IOutboxWriter
{
    private readonly UserModuleDbContext _dbContext = dbContext;

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<UserModuleOutboxMessage>(integrationEvent);
        _dbContext.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}