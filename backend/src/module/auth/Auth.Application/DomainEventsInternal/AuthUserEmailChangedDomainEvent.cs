namespace Auth.Application.DomainEventsInternal;

public record AuthUserEmailChangedDomainEvent(Guid UserId, string NewEmail) : BaseDomainEvent, INotification;
