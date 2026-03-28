namespace Auth.Application.DomainEventsInternal;

public record AuthUserActivatedDomainEvent(Guid UserId) : BaseDomainEvent, INotification;
