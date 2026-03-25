using HanziAnhVu.Shared.EventBus;

namespace Auth.Infrastructure.Outbox;

public sealed class OutboxMessageWriter : IOutboxWriter
{
    private readonly OutboxMessageDbContext _dbContext;

    public OutboxMessageWriter(OutboxMessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Add(OutboxMessageMapper.Create(integrationEvent));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
