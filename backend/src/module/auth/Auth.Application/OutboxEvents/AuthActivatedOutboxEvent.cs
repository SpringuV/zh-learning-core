namespace Auth.Application.OutboxEvents;

public sealed class AuthActivatedOutboxEvent(IOutboxWriter outboxWriter)
    : INotificationHandler<AuthUserActivatedDomainEvent>
{
    public Task Handle(AuthUserActivatedDomainEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserActivatedIntegrationEvent(notification.UserId),
            cancellationToken);
}
