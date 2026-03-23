namespace Auth.Application.DomainEventsInternal
{
    public record AuthUserPasswordChangedDomainEvent(Guid UserId) : BaseDomainEvent, INotification;
}
