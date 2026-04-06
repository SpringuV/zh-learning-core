using HanziAnhVu.Shared.Application;

namespace Auth.Application.DomainEventHandlers;

public sealed class AuthDeactivatedOutboxEvent(IOutboxWriter outboxWriter)
    : INotificationHandler<UserDeactivatedDomainEvent>
{
    public Task Handle(UserDeactivatedDomainEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserDeactivateIntegrationEvent(notification.UserId),
            cancellationToken);
}