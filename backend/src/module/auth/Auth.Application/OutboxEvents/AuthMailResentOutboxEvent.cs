namespace Auth.Application.OutboxEvents;

public sealed class AuthMailResentOutboxEvent(IOutboxWriter outboxWriter) : INotificationHandler<AuthUserMailResentEvent>
{
    public Task Handle(AuthUserMailResentEvent notify, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new UserMailResentIntegrationEvent(notify.Email, notify.ActivationLink, notify.ResendLink, notify.ExpiredActivation),
            cancellationToken);
}
