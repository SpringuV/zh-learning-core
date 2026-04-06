using HanziAnhVu.Shared.Application;
using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Auth.Infrastructure.Identity;

public class OutboxMessageWriter(AuthIdentityDbContext dbContext) : IOutboxWriter
{
    private readonly AuthIdentityDbContext _dbContext = dbContext;

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<AuthOutboxMessage>(integrationEvent);
        _dbContext.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}
