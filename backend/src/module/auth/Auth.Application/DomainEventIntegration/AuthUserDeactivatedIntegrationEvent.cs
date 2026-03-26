using Auth.Application.DomainEventsInternal;

namespace Auth.Application.DomainEventHandlers;

public sealed class AuthUserDeactivatedIntegrationEvent(IEventBus @eventBus) : INotificationHandler<UserDeactivatedDomainEvent>
{
    public Task Handle(UserDeactivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        eventBus.PublishAsync(new UserDeactivateIntegrationEvent(notification.UserId), cancellationToken);
        return Task.CompletedTask;
    }
}