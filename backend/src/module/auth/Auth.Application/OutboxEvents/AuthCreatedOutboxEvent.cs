using HanziAnhVu.Shared.Application;

namespace Auth.Application.DomainEventHandlers;

// AuthUserCreatedDomainEventHandler is responsible for handling the AuthUserCreatedDomainEvent
// and publishing a UserRegisteredIntegrationEvent to the event bus.

// first AuthUserCreatedDomainEvent is internal event in module auth,
// it is raised when a new user is created in the system.
// This event contains information about the newly created user, such as UserId, Email, and UserName.


// and then AuthUserCreatedDomainEventHandler will listen to this event
// and handle it by publishing a UserRegisteredIntegrationEvent to the event bus,
// any module subscribed to this event can react accordingly and can perform additional actions itself.
public sealed class AuthCreatedOutboxEvent(IOutboxWriter outboxWriter)
    : INotificationHandler<AuthUserCreatedDomainEvent>
{
    public Task Handle(AuthUserCreatedDomainEvent notify, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserRegisteredIntegrationEvent(notify.UserId, notify.Email, notify.UserName, notify.CreatedAt, notify.ActivateCode, notify.ActivationLink, notify.ResendLink),
            cancellationToken);
}
