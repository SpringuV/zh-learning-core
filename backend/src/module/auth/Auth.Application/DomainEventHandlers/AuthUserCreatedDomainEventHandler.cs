namespace Auth.Application.DomainEventHandlers;

public sealed class AuthUserCreatedDomainEventHandler(IEventBus eventBus)
    : INotificationHandler<AuthUserCreatedDomainEvent>
{
    public Task Handle(AuthUserCreatedDomainEvent notification, CancellationToken cancellationToken)
        => eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(notification.UserId, notification.Email, notification.UserName),
            cancellationToken);

}
