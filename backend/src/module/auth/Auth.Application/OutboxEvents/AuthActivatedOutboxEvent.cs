using Auth.Application.DomainEventsInternal;

namespace Auth.Application.DomainEventHandlers;

public sealed class AuthActivatedOutboxEvent(IEventBus @eventBus) : INotificationHandler<AuthUserActivatedDomainEvent>
{
    public Task Handle(AuthUserActivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        eventBus.PublishAsync(new UserActivatedIntegrationEvent(notification.UserId), cancellationToken);
        return Task.CompletedTask;
    }
}
