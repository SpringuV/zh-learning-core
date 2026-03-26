namespace Auth.Application.DomainEventHandlers;

// AuthUserCreatedDomainEventHandler is responsible for handling the AuthUserCreatedDomainEvent
// and publishing a UserRegisteredIntegrationEvent to the event bus.

// first AuthUserCreatedDomainEvent is internal event in module auth,
// it is raised when a new user is created in the system.
// This event contains information about the newly created user, such as UserId, Email, and UserName.


// and then AuthUserCreatedDomainEventHandler will listen to this event
// and handle it by publishing a UserRegisteredIntegrationEvent to the event bus,
// any module subscribed to this event can react accordingly and can perform additional actions itself.
public sealed class AuthUserCreatedIntegrationEvent(IEventBus eventBus)
    : INotificationHandler<AuthUserCreatedDomainEvent>
{
    public Task Handle(AuthUserCreatedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(notification.UserId, notification.Email, notification.UserName, notification.CreatedAt, notification.ActivateCode),
            cancellationToken);
}
