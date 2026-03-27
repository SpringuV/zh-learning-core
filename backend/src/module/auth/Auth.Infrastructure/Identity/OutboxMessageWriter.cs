using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Auth.Infrastructure.Identity;

public class OutboxMessageWriter : IOutboxWriter
{
    private readonly AuthIdentityDbContext _dbContext;

    public OutboxMessageWriter(AuthIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<AuthOutboxMessage>(integrationEvent);
        _dbContext.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}
