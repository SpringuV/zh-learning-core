namespace Auth.Application.DomainEventsInternal
{
    public record AuthUserPasswordChangedDomainEvent(Guid UserId, DateTime LastTimeChange) : BaseDomainEvent, INotification;
}
