
namespace Auth.Application.DomainEventIntegration;

public sealed class AuthProfileUpdatedOutboxEvent(IOutboxWriter outboxWriter) : INotificationHandler<AuthUserProfileUpdatedEvent>
{
    public Task Handle(AuthUserProfileUpdatedEvent notify, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserProfileUpdatedIntegrationEvent(notify.UserId, notify.NewPhoneNumber, notify.NewAvatarUrl, notify.UpdatedAt),
            cancellationToken);
}
