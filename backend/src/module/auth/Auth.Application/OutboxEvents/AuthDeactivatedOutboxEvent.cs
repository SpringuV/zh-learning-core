namespace Auth.Application.OutboxEvents;

public sealed class AuthDeactivatedOutboxEvent(IOutboxWriter outboxWriter)
    : INotificationHandler<UserDeactivatedDomainEvent>
{
    public Task Handle(UserDeactivatedDomainEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserDeactivateIntegrationEvent(notification.UserId),
            cancellationToken);
}